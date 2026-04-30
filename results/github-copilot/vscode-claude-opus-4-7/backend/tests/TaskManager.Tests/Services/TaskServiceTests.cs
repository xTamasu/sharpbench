// TaskServiceTests.cs
// Unit tests for TaskService covering CRUD plus ownership enforcement on delete.
using FluentValidation;
using Moq;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Application.Validation;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly IValidator<CreateTaskRequest> _createV = new CreateTaskRequestValidator();
    private readonly IValidator<UpdateTaskRequest> _updateV = new UpdateTaskRequestValidator();

    private TaskService Build() => new(_tasks.Object, _users.Object, _createV, _updateV);

    private static User MakeUser(Guid? id = null) =>
        new() { Id = id ?? Guid.NewGuid(), Email = "u@example.com", DisplayName = "U", PasswordHash = "h" };

    [Fact]
    public async Task List_PassesFiltersToRepository()
    {
        // Arrange
        _tasks.Setup(r => r.QueryAsync(TaskItemStatus.Todo, TaskPriority.High, null, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await Build().ListAsync(TaskItemStatus.Todo, TaskPriority.High, null);

        // Assert
        Assert.Empty(result);
        _tasks.VerifyAll();
    }

    [Fact]
    public async Task Get_NotFound_Throws()
    {
        _tasks.Setup(r => r.GetByIdWithCommentsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((TaskItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => Build().GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Get_HappyPath_ReturnsDetail()
    {
        var creator = MakeUser();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(), Title = "t", CreatedById = creator.Id, CreatedBy = creator,
            Status = TaskItemStatus.Todo, Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _tasks.Setup(r => r.GetByIdWithCommentsAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await Build().GetAsync(task.Id);
        Assert.Equal(task.Id, result.Id);
    }

    [Fact]
    public async Task Create_HappyPath_PersistsAndReturnsDto()
    {
        // Arrange
        var creator = MakeUser();
        var req = new CreateTaskRequest("New task", "desc", TaskItemStatus.Todo, TaskPriority.Medium, null, null);
        _users.Setup(r => r.GetByIdAsync(creator.Id, It.IsAny<CancellationToken>())).ReturnsAsync(creator);
        _tasks.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _tasks.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _tasks.Setup(r => r.GetByIdWithCommentsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((Guid id, CancellationToken _) => new TaskItem
              {
                  Id = id, Title = "New task", CreatedById = creator.Id, CreatedBy = creator,
                  Status = TaskItemStatus.Todo, Priority = TaskPriority.Medium,
                  CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
              });

        // Act
        var result = await Build().CreateAsync(req, creator.Id);

        // Assert
        Assert.Equal("New task", result.Title);
        _tasks.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_EmptyTitle_ThrowsValidation()
    {
        var req = new CreateTaskRequest("", null, TaskItemStatus.Todo, TaskPriority.Low, null, null);
        await Assert.ThrowsAsync<ValidationFailedException>(() => Build().CreateAsync(req, Guid.NewGuid()));
    }

    [Fact]
    public async Task Create_AssigneeMissing_ThrowsValidation()
    {
        var req = new CreateTaskRequest("ok", null, TaskItemStatus.Todo, TaskPriority.Low, null, Guid.NewGuid());
        _users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<ValidationFailedException>(() => Build().CreateAsync(req, Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_BumpsUpdatedAt()
    {
        // Arrange
        var creator = MakeUser();
        var existing = new TaskItem
        {
            Id = Guid.NewGuid(), Title = "old", CreatedById = creator.Id, CreatedBy = creator,
            Status = TaskItemStatus.Todo, Priority = TaskPriority.Low,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _tasks.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _tasks.Setup(r => r.GetByIdWithCommentsAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var req = new UpdateTaskRequest("new title", null, TaskItemStatus.Done, TaskPriority.High, null, null);

        // Act
        var before = existing.UpdatedAt;
        var result = await Build().UpdateAsync(existing.Id, req, creator.Id);

        // Assert
        Assert.True(existing.UpdatedAt > before);
        Assert.Equal("new title", existing.Title);
        Assert.Equal(TaskItemStatus.Done, existing.Status);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        _tasks.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);
        var req = new UpdateTaskRequest("t", null, TaskItemStatus.Todo, TaskPriority.Low, null, null);
        await Assert.ThrowsAsync<NotFoundException>(() => Build().UpdateAsync(Guid.NewGuid(), req, Guid.NewGuid()));
    }

    [Fact]
    public async Task Delete_OnlyCreatorAllowed_ThrowsForOthers()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var otherUser = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), CreatedById = creator };
        _tasks.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => Build().DeleteAsync(task.Id, otherUser));
        _tasks.Verify(r => r.Remove(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Creator_Removes()
    {
        var creator = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), CreatedById = creator };
        _tasks.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        await Build().DeleteAsync(task.Id, creator);

        _tasks.Verify(r => r.Remove(task), Times.Once);
        _tasks.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NotFound_Throws()
    {
        _tasks.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => Build().DeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }
}
