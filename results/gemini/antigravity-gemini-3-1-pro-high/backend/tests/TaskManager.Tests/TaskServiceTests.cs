using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain;
using Xunit;

namespace TaskManager.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<IExtendedTaskRepository> _taskRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepositoryMock = new Mock<IExtendedTaskRepository>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _taskService = new TaskService(_taskRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldReturnTask_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateTaskRequest("New Task", "Desc", Priority.High, null, null);

            // Act
            var result = await _taskService.CreateTaskAsync(userId, request);

            // Assert
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(userId, result.CreatedById);
            _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskEntity>()), Times.Once);
            _taskRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTask_WhenExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var taskEntity = new TaskEntity { Id = taskId, Title = "Existing Task", Comments = new List<TaskComment>() };
            _taskRepositoryMock.Setup(r => r.GetTaskWithCommentsAsync(taskId)).ReturnsAsync(taskEntity);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.Equal(taskId, result.Id);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldThrowKeyNotFound_WhenNotExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _taskRepositoryMock.Setup(r => r.GetTaskWithCommentsAsync(taskId)).ReturnsAsync((TaskEntity?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.GetTaskByIdAsync(taskId));
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldUpdate_WhenExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingTask = new TaskEntity { Id = taskId, Title = "Old", CreatedById = userId };
            var request = new UpdateTaskRequest("New", "Desc", Status.InProgress, Priority.Low, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(existingTask);

            // Act
            var result = await _taskService.UpdateTaskAsync(taskId, userId, request);

            // Assert
            Assert.Equal("New", result.Title);
            Assert.Equal(Status.InProgress, result.Status);
            _taskRepositoryMock.Verify(r => r.Update(existingTask), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldDelete_WhenUserIsCreator()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingTask = new TaskEntity { Id = taskId, CreatedById = userId };

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(existingTask);

            // Act
            await _taskService.DeleteTaskAsync(taskId, userId);

            // Assert
            _taskRepositoryMock.Verify(r => r.Remove(existingTask), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldThrowUnauthorized_WhenUserIsNotCreator()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var existingTask = new TaskEntity { Id = taskId, CreatedById = creatorId };

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(existingTask);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.DeleteTaskAsync(taskId, otherUserId));
        }
    }
}
