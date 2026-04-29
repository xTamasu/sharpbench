// Unit tests for TaskService: covers CRUD operations, not-found, and ownership checks.
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _taskRepoMock = new Mock<ITaskRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new TaskService(_taskRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Task 1",
                Status = TaskItemStatus.Todo,
                Priority = TaskPriority.High,
                CreatedById = Guid.NewGuid(),
                CreatedBy = new User { DisplayName = "User 1" },
                Comments = new List<TaskComment>()
            }
        };
        _taskRepoMock.Setup(r => r.GetFilteredAsync(TaskItemStatus.Todo, null, null))
            .ReturnsAsync(tasks);

        // Act
        var result = await _sut.GetAllAsync(TaskItemStatus.Todo, null, null);

        // Assert
        var taskList = result.ToList();
        Assert.Single(taskList);
        Assert.Equal("Task 1", taskList[0].Title);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTask_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.Medium,
            CreatedById = Guid.NewGuid(),
            CreatedBy = new User { DisplayName = "Creator" },
            Comments = new List<TaskComment>()
        };
        _taskRepoMock.Setup(r => r.GetByIdWithCommentsAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _sut.GetByIdAsync(taskId);

        // Assert
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdWithCommentsAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(taskId));
    }

    [Fact]
    public async Task CreateAsync_HappyPath_ReturnsCreatedTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Description",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Low
        };

        _taskRepoMock.Setup(r => r.GetByIdWithCommentsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                CreatedById = userId,
                CreatedBy = new User { DisplayName = "Creator" },
                Comments = new List<TaskComment>()
            });

        // Act
        var result = await _sut.CreateAsync(request, userId);

        // Assert
        Assert.Equal("New Task", result.Title);
        _taskRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
        _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Old Title",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Low,
            CreatedById = Guid.NewGuid()
        };
        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);
        _taskRepoMock.Setup(r => r.GetByIdWithCommentsAsync(taskId))
            .ReturnsAsync(new TaskItem
            {
                Id = taskId,
                Title = "Updated Title",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.High,
                CreatedById = existingTask.CreatedById,
                CreatedBy = new User { DisplayName = "Creator" },
                Comments = new List<TaskComment>()
            });

        var request = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.High
        };

        // Act
        var result = await _sut.UpdateAsync(taskId, request);

        // Assert
        Assert.Equal("Updated Title", result.Title);
        _taskRepoMock.Verify(r => r.Update(It.IsAny<TaskItem>()), Times.Once);
        _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var request = new UpdateTaskRequest { Title = "Title" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(taskId, request));
    }

    [Fact]
    public async Task DeleteAsync_HappyPath_Creator_DeletesTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            CreatedById = userId
        };
        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act
        await _sut.DeleteAsync(taskId, userId);

        // Assert
        _taskRepoMock.Verify(r => r.Remove(task), Times.Once);
        _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonCreator_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            CreatedById = creatorId
        };
        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act & Assert — only the creator can delete
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(taskId, otherUserId));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(taskId, Guid.NewGuid()));
    }
}
