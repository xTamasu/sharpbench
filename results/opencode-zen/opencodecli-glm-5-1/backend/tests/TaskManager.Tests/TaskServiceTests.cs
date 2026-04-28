// Unit tests for TaskService covering CRUD, filtering, and ownership checks
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _taskRepoMock = new Mock<ITaskRepository>();
        _sut = new TaskService(_taskRepoMock.Object);
    }

    private static DomainTask CreateSampleTask(Guid? creatorId = null)
    {
        var id = creatorId ?? Guid.NewGuid();
        return new DomainTask
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "A description",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Medium,
            CreatedById = id,
            CreatedBy = new User { Id = id, DisplayName = "Creator" },
            AssignedToId = null,
            AssignedTo = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Comments = new List<TaskComment>()
        };
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<DomainTask> { CreateSampleTask(), CreateSampleTask() };
        _taskRepoMock.Setup(r => r.GetFilteredAsync(null, null, null)).ReturnsAsync(tasks);

        // Act
        var result = await _sut.GetAllAsync(null, null, null);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new List<DomainTask> { CreateSampleTask() };
        _taskRepoMock.Setup(r => r.GetFilteredAsync(TaskItemStatus.Done, null, null)).ReturnsAsync(tasks);

        // Act
        var result = await _sut.GetAllAsync(TaskItemStatus.Done, null, null);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsTaskDetail()
    {
        // Arrange
        var task = CreateSampleTask();
        _taskRepoMock.Setup(r => r.GetWithCommentsAsync(task.Id)).ReturnsAsync(task);

        // Act
        var result = await _sut.GetByIdAsync(task.Id);

        // Assert
        Assert.Equal(task.Id, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetWithCommentsAsync(It.IsAny<Guid>())).ReturnsAsync((DomainTask?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = CreateSampleTask(userId);
        var request = new CreateTaskRequest("New Task", "Desc", TaskItemStatus.Todo, TaskPriority.High, null, null);

        _taskRepoMock.Setup(r => r.AddAsync(It.IsAny<DomainTask>())).ReturnsAsync((DomainTask t) => t);
        _taskRepoMock.Setup(r => r.GetWithCommentsAsync(It.IsAny<Guid>())).ReturnsAsync(task);

        // Act
        var result = await _sut.CreateAsync(request, userId);

        // Assert
        _taskRepoMock.Verify(r => r.AddAsync(It.IsAny<DomainTask>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = CreateSampleTask(userId);
        var request = new UpdateTaskRequest("Updated Title", null, TaskItemStatus.InProgress, null, null, null);

        _taskRepoMock.Setup(r => r.GetWithCommentsAsync(task.Id)).ReturnsAsync(task);
        _taskRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DomainTask>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(task.Id, request, userId);

        // Assert
        _taskRepoMock.Verify(r => r.UpdateAsync(It.IsAny<DomainTask>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTask_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetWithCommentsAsync(It.IsAny<Guid>())).ReturnsAsync((DomainTask?)null);
        var request = new UpdateTaskRequest("Title", null, null, null, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(Guid.NewGuid(), request, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_CreatorDeletes_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = CreateSampleTask(userId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        _taskRepoMock.Setup(r => r.DeleteAsync(It.IsAny<DomainTask>())).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(task.Id, userId);

        // Assert
        _taskRepoMock.Verify(r => r.DeleteAsync(It.IsAny<DomainTask>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonCreatorDeletes_ThrowsForbiddenException()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = CreateSampleTask(creatorId);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(task.Id, otherUserId));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTask_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DomainTask?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }
}