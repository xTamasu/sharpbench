using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain;
using Xunit;

namespace TaskManager.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<IRepository<TaskComment>> _commentRepositoryMock;
        private readonly Mock<IExtendedTaskRepository> _taskRepositoryMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<IRepository<TaskComment>>();
            _taskRepositoryMock = new Mock<IExtendedTaskRepository>();
            _commentService = new CommentService(_commentRepositoryMock.Object, _taskRepositoryMock.Object);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldAdd_WhenTaskExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateCommentRequest("Test comment");
            
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(new TaskEntity { Id = taskId });

            // Act
            var result = await _commentService.AddCommentAsync(taskId, userId, request);

            // Assert
            Assert.Equal("Test comment", result.Body);
            Assert.Equal(taskId, result.TaskId);
            Assert.Equal(userId, result.AuthorId);
            _commentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskComment>()), Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldThrowNotFound_WhenTaskNotExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskEntity?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _commentService.AddCommentAsync(taskId, Guid.NewGuid(), new CreateCommentRequest("")));
        }

        [Fact]
        public async Task EditCommentAsync_ShouldEdit_WhenUserIsAuthor()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingComment = new TaskComment { Id = commentId, TaskId = taskId, AuthorId = userId, Body = "Old" };
            var request = new UpdateCommentRequest("New");

            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(existingComment);

            // Act
            var result = await _commentService.EditCommentAsync(taskId, commentId, userId, request);

            // Assert
            Assert.Equal("New", result.Body);
            Assert.NotNull(result.EditedAt);
        }

        [Fact]
        public async Task EditCommentAsync_ShouldThrowUnauthorized_WhenUserIsNotAuthor()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var existingComment = new TaskComment { Id = commentId, TaskId = taskId, AuthorId = authorId };

            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(existingComment);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _commentService.EditCommentAsync(taskId, commentId, otherUserId, new UpdateCommentRequest("New")));
        }
    }
}
