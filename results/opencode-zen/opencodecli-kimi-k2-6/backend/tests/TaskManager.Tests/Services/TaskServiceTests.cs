using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.Services;

/// <summary>
/// Unit tests for the TaskService covering CRUD operations, filtering, and ownership rules.
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<TaskItem>> _taskRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<IRepository<TaskItem>>();
        _unitOfWorkMock.Setup(u => u.Repository<TaskItem>()).Returns(_taskRepositoryMock.Object);
        _taskService = new TaskService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.Todo, Priority = Priority.Low },
            new() { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.Done, Priority = Priority.High }
        };

        _taskRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetAllAsync(null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.Todo, Priority = Priority.Low },
            new() { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.Done, Priority = Priority.High }
        };

        _taskRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var filter = new TaskFilterRequest { Status = TaskStatus.Done };

        // Act
        var result = await _taskService.GetAllAsync(filter);

        // Assert
        Assert.Single(result);
        Assert.Equal(TaskStatus.Done, result[0].Status);
    }

    [Fact]
    public async Task GetAllAsync_WithPriorityFilter_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatus.Todo, Priority = Priority.Low },
            new() { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatus.InProgress, Priority = Priority.High }
        };

        _taskRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var filter = new TaskFilterRequest { Priority = Priority.High };

        // Act
        var result = await _taskService.GetAllAsync(filter);

        // Assert
        Assert.Single(result);
        Assert.Equal(Priority.High, result[0].Priority);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTask_ReturnsTaskDetail()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Status = TaskStatus.Todo,
            Priority = Priority.Medium,
            CreatedBy = new User { DisplayName = "Creator" },
            Comments = new List<TaskComment>()
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.GetByIdAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.GetByIdAsync(taskId));
        Assert.Contains(taskId.ToString(), exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Description",
            Status = TaskStatus.Todo,
            Priority = Priority.Medium
        };

        _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new TaskItem
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                CreatedById = userId,
                CreatedBy = new User { DisplayName = "Test User" }
            });

        // Act
        var result = await _taskService.CreateAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Status, result.Status);
        _taskRepositoryMock.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.CreatedById == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Old Title",
            Status = TaskStatus.Todo,
            Priority = Priority.Low,
            CreatedById = userId
        };

        var request = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = TaskStatus.InProgress,
            Priority = Priority.High
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        // Act
        var result = await _taskService.UpdateAsync(taskId, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Status, result.Status);
        Assert.Equal(request.Priority, result.Priority);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Status = TaskStatus.Todo,
            Priority = Priority.Low
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.UpdateAsync(taskId, request, userId));
        Assert.Contains(taskId.ToString(), exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_TaskCreator_DeletesTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task to Delete",
            CreatedById = creatorId
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _taskService.DeleteAsync(taskId, creatorId);

        // Assert
        _taskRepositoryMock.Verify(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotCreator_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task to Delete",
            CreatedById = creatorId
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.DeleteAsync(taskId, otherUserId));
        Assert.Equal("Only the task creator can delete this task.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.DeleteAsync(taskId, userId));
        Assert.Contains(taskId.ToString(), exception.Message);
    }
}
