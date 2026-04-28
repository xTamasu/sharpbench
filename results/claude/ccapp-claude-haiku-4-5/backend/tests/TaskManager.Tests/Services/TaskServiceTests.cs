using Moq;
using TaskManager.Application.Dto.Task;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;
using DomainTask = TaskManager.Domain.Entities.Task;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Tests.Services;

/// <summary>
/// Unit tests for TaskService.
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<IRepository<DomainTask>> _taskRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<IRepository<DomainTask>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _taskService = new TaskService(_taskRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllTasksAsync_ReturnsAllTasks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Test User" };
        var tasks = new List<DomainTask>
        {
            new DomainTask { Id = Guid.NewGuid(), Title = "Task 1", CreatedBy = user, CreatedById = userId },
            new DomainTask { Id = Guid.NewGuid(), Title = "Task 2", CreatedBy = user, CreatedById = userId }
        };
        _taskRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllTasksAsync_FiltersByStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Test User" };
        var tasks = new List<DomainTask>
        {
            new DomainTask { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskManager.Domain.Entities.TaskStatus.Todo, CreatedBy = user, CreatedById = userId },
            new DomainTask { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskManager.Domain.Entities.TaskStatus.Done, CreatedBy = user, CreatedById = userId }
        };
        _taskRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetAllTasksAsync(status: "0");

        // Assert
        Assert.Single(result);
        Assert.Equal(TaskStatus.Todo, (TaskStatus)result.First().Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskByIdAsync_WithValidId_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Test User" };
        var task = new DomainTask { Id = taskId, Title = "Test Task", CreatedBy = user, CreatedById = userId };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((DomainTask?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.GetTaskByIdAsync(taskId));
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_WithValidData_CreatesTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest { Title = "New Task", Description = "Description", Priority = 1 };
        _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainTask>())).ReturnsAsync((DomainTask t) => t);
        _taskRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _taskService.CreateTaskAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Task", result.Title);
        Assert.Equal(userId, result.CreatedById);
        _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainTask>()), Times.Once);
        _taskRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskAsync_WithCorrectOwner_UpdatesTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Test User" };
        var task = new DomainTask { Id = taskId, Title = "Old Title", CreatedBy = user, CreatedById = userId };
        var request = new UpdateTaskRequest { Title = "New Title" };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DomainTask>())).Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, request, userId);

        // Assert
        Assert.Equal("New Title", result.Title);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DomainTask>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskAsync_WithWrongOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var user = new User { Id = creatorId, DisplayName = "Test User" };
        var task = new DomainTask { Id = taskId, Title = "Task", CreatedBy = user, CreatedById = creatorId };
        var request = new UpdateTaskRequest { Title = "New Title" };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.UpdateTaskAsync(taskId, request, otherUserId));
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTaskAsync_WithCorrectOwner_DeletesTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Test User" };
        var task = new DomainTask { Id = taskId, CreatedBy = user, CreatedById = userId };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        _taskRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<DomainTask>())).Returns(System.Threading.Tasks.Task.CompletedTask);
        _taskRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _taskService.DeleteTaskAsync(taskId, userId);

        // Assert
        _taskRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<DomainTask>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTaskAsync_WithWrongOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var user = new User { Id = creatorId, DisplayName = "Test User" };
        var task = new DomainTask { Id = taskId, CreatedBy = user, CreatedById = creatorId };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.DeleteTaskAsync(taskId, otherUserId));
    }
}
