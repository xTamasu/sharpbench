// TaskServiceTests — tests for all task CRUD operations including ownership checks and filtering.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using Xunit;

namespace TaskManager.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<IRepository<TaskItem>> _mockTaskRepo;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<TaskComment>> _mockCommentRepo;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _mockTaskRepo = new Mock<IRepository<TaskItem>>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockCommentRepo = new Mock<IRepository<TaskComment>>();
        _service = new TaskService(_mockTaskRepo.Object, _mockUserRepo.Object, _mockCommentRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_HappyPath_ReturnsAllTasks()
    {
        // Arrange
        var filter = new TaskListFilter();
        var tasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatusEnum.Todo, Priority = TaskPriorityEnum.High, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatusEnum.Done, Priority = TaskPriorityEnum.Low, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        }.AsQueryable();

        _mockTaskRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_FilterByStatus_ReturnsFilteredTasks()
    {
        // Arrange
        var filter = new TaskListFilter { Status = TaskStatusEnum.Done };
        var tasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskStatusEnum.Todo, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskStatusEnum.Done, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        }.AsQueryable();

        _mockTaskRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(TaskStatusEnum.Done);
    }

    [Fact]
    public async Task GetAllAsync_FilterByPriority_ReturnsFilteredTasks()
    {
        // Arrange
        var filter = new TaskListFilter { Priority = TaskPriorityEnum.High };
        var tasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Priority = TaskPriorityEnum.Low, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Priority = TaskPriorityEnum.High, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        }.AsQueryable();

        _mockTaskRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Priority.Should().Be(TaskPriorityEnum.High);
    }

    [Fact]
    public async Task GetByIdAsync_HappyPath_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Status = TaskStatusEnum.Todo,
            Priority = TaskPriorityEnum.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        var result = await _service.GetByIdAsync(taskId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(taskId);
        result.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(taskId));
    }

    [Fact]
    public async Task CreateAsync_HappyPath_CreatesAndReturnsTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Description",
            Status = TaskStatusEnum.Todo,
            Priority = TaskPriorityEnum.High
        };
        var user = new User { Id = userId, Email = "test@test.com", DisplayName = "Test User", CreatedAt = DateTime.UtcNow };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockTaskRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken ct) => t);

        // Act
        var result = await _service.CreateAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        result.CreatedById.Should().Be(userId);
        _mockTaskRepo.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CreatorNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest { Title = "New Task", Status = TaskStatusEnum.Todo, Priority = TaskPriorityEnum.Medium };
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(userId, request));
    }

    [Fact]
    public async Task CreateAsync_AssignedUserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var assignedId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Status = TaskStatusEnum.Todo,
            Priority = TaskPriorityEnum.Medium,
            AssignedToId = assignedId
        };
        var user = new User { Id = userId, Email = "test@test.com", DisplayName = "Test", CreatedAt = DateTime.UtcNow };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.GetByIdAsync(assignedId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(userId, request));
    }

    [Fact]
    public async Task UpdateAsync_HappyPath_UpdatesTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Old Title",
            Status = TaskStatusEnum.Todo,
            Priority = TaskPriorityEnum.Low,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var request = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Status = TaskStatusEnum.InProgress,
            Priority = TaskPriorityEnum.High
        };

        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        var result = await _service.UpdateAsync(taskId, userId, request);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Status.Should().Be(TaskStatusEnum.InProgress);
        result.Priority.Should().Be(TaskPriorityEnum.High);
        _mockTaskRepo.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTaskRequest { Title = "Updated", Status = TaskStatusEnum.Todo, Priority = TaskPriorityEnum.Medium };
        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(taskId, userId, request));
    }

    [Fact]
    public async Task DeleteAsync_HappyPath_DeletesTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task",
            Status = TaskStatusEnum.Todo,
            Priority = TaskPriorityEnum.Medium,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        await _service.DeleteAsync(taskId, userId);

        // Assert
        _mockTaskRepo.Verify(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(taskId, userId));
    }

    [Fact]
    public async Task DeleteAsync_WrongOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task",
            Status = TaskStatusEnum.Todo,
            Priority = TaskPriorityEnum.Medium,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteAsync(taskId, otherUserId));
    }
}
