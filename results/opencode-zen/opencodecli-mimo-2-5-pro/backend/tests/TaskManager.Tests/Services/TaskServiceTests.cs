// Unit tests for TaskService covering CRUD operations and ownership checks.

using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using Xunit;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly TaskService _taskService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _taskService = new TaskService(_taskRepositoryMock.Object);
    }

    private TaskItem CreateTestTask(Guid? createdById = null)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Todo,
            Priority = Priority.Medium,
            CreatedById = createdById ?? _userId,
            CreatedBy = new User
            {
                Id = createdById ?? _userId,
                Email = "creator@test.com",
                DisplayName = "Creator",
                PasswordHash = "hash"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Comments = new List<TaskComment>()
        };
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new List<TaskItem> { CreateTestTask() };
        _taskRepositoryMock.Setup(r => r.GetFilteredAsync(null, null, null))
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetTasksAsync(null, null, null);

        // Assert
        Assert.Single(result);
        _taskRepositoryMock.Verify(r => r.GetFilteredAsync(null, null, null), Times.Once);
    }

    [Fact]
    public async Task GetTaskByIdAsync_TaskExists_ReturnsTaskDetail()
    {
        // Arrange
        var task = CreateTestTask();
        _taskRepositoryMock.Setup(r => r.GetWithCommentsAsync(task.Id)).ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _taskRepositoryMock.Setup(r => r.GetWithCommentsAsync(nonExistentId))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _taskService.GetTaskByIdAsync(nonExistentId));
    }

    [Fact]
    public async Task CreateTaskAsync_ValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        var request = new CreateTaskRequest("New Task", "Description", Priority.High, null, null);
        _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) => t);
        _taskRepositoryMock.Setup(r => r.GetWithCommentsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => CreateTestTask());

        // Act
        var result = await _taskService.CreateTaskAsync(request, _userId);

        // Assert
        Assert.NotNull(result);
        _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
        _taskRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_TaskExists_ReturnsUpdatedTask()
    {
        // Arrange
        var task = CreateTestTask();
        var request = new UpdateTaskRequest("Updated Title", "Updated Desc", TaskStatus.InProgress, Priority.High, null, null);
        _taskRepositoryMock.Setup(r => r.GetWithCommentsAsync(task.Id)).ReturnsAsync(task);

        // Act
        var result = await _taskService.UpdateTaskAsync(task.Id, request, _userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        _taskRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateTaskRequest("Title", null, TaskStatus.Todo, Priority.Low, null, null);
        _taskRepositoryMock.Setup(r => r.GetWithCommentsAsync(nonExistentId))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _taskService.UpdateTaskAsync(nonExistentId, request, _userId));
    }

    [Fact]
    public async Task DeleteTaskAsync_CreatorDeletes_Succeeds()
    {
        // Arrange
        var task = CreateTestTask(createdById: _userId);
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        // Act
        await _taskService.DeleteTaskAsync(task.Id, _userId);

        // Assert
        _taskRepositoryMock.Verify(r => r.DeleteAsync(task), Times.Once);
        _taskRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_NonCreator_ThrowsUnauthorizedAccessException()
    {
        // Arrange: task created by _userId, but _otherUserId tries to delete
        var task = CreateTestTask(createdById: _userId);
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _taskService.DeleteTaskAsync(task.Id, _otherUserId));
        Assert.Equal("Only the task creator can delete this task.", exception.Message);
    }

    [Fact]
    public async Task DeleteTaskAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(nonExistentId))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _taskService.DeleteTaskAsync(nonExistentId, _userId));
    }
}
