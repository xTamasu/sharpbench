// CommentServiceTests — tests for comment CRUD including author-based edit/delete enforcement.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Comments;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using Xunit;

namespace TaskManager.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<IRepository<TaskItem>> _mockTaskRepo;
    private readonly Mock<IRepository<TaskComment>> _mockCommentRepo;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _mockTaskRepo = new Mock<IRepository<TaskItem>>();
        _mockCommentRepo = new Mock<IRepository<TaskComment>>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _service = new CommentService(_mockTaskRepo.Object, _mockCommentRepo.Object, _mockUserRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_HappyPath_CreatesComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new CreateCommentRequest { Body = "This is a comment" };
        var task = new TaskItem { Id = taskId, Title = "Task", Status = TaskStatusEnum.Todo, Priority = TaskPriorityEnum.Medium, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var author = new User { Id = authorId, Email = "test@test.com", DisplayName = "Test User", CreatedAt = DateTime.UtcNow };

        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _mockUserRepo.Setup(r => r.GetByIdAsync(authorId, It.IsAny<CancellationToken>())).ReturnsAsync(author);
        _mockCommentRepo.Setup(r => r.AddAsync(It.IsAny<TaskComment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskComment c, CancellationToken ct) => c);

        // Act
        var result = await _service.CreateAsync(taskId, authorId, request);

        // Assert
        result.Should().NotBeNull();
        result.Body.Should().Be("This is a comment");
        result.AuthorId.Should().Be(authorId);
        result.TaskId.Should().Be(taskId);
        _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<TaskComment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new CreateCommentRequest { Body = "Comment" };
        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(taskId, authorId, request));
    }

    [Fact]
    public async Task CreateAsync_AuthorNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new CreateCommentRequest { Body = "Comment" };
        var task = new TaskItem { Id = taskId, Title = "Task", Status = TaskStatusEnum.Todo, Priority = TaskPriorityEnum.Medium, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _mockUserRepo.Setup(r => r.GetByIdAsync(authorId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(taskId, authorId, request));
    }

    [Fact]
    public async Task UpdateAsync_HappyPath_UpdatesCommentAndSetsEditedAt()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Old body",
            CreatedAt = DateTime.UtcNow
        };
        var author = new User { Id = authorId, Email = "test@test.com", DisplayName = "Test", CreatedAt = DateTime.UtcNow };
        var request = new UpdateCommentRequest { Body = "Updated body" };

        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
        _mockUserRepo.Setup(r => r.GetByIdAsync(authorId, It.IsAny<CancellationToken>())).ReturnsAsync(author);

        // Act
        var result = await _service.UpdateAsync(taskId, commentId, authorId, request);

        // Assert
        result.Body.Should().Be("Updated body");
        result.EditedAt.Should().NotBeNull();
        _mockCommentRepo.Verify(r => r.UpdateAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CommentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Body = "Updated" };
        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskComment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(taskId, commentId, authorId, request));
    }

    [Fact]
    public async Task UpdateAsync_WrongAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Body",
            CreatedAt = DateTime.UtcNow
        };
        var request = new UpdateCommentRequest { Body = "Updated" };

        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateAsync(taskId, commentId, otherUserId, request));
    }

    [Fact]
    public async Task UpdateAsync_WrongTask_ThrowsInvalidOperationException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var otherTaskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = otherTaskId,
            AuthorId = authorId,
            Body = "Body",
            CreatedAt = DateTime.UtcNow
        };
        var request = new UpdateCommentRequest { Body = "Updated" };

        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(taskId, commentId, authorId, request));
    }

    [Fact]
    public async Task DeleteAsync_HappyPath_DeletesComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Body",
            CreatedAt = DateTime.UtcNow
        };

        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        // Act
        await _service.DeleteAsync(taskId, commentId, authorId);

        // Assert
        _mockCommentRepo.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CommentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskComment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(taskId, commentId, authorId));
    }

    [Fact]
    public async Task DeleteAsync_WrongAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Body",
            CreatedAt = DateTime.UtcNow
        };

        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteAsync(taskId, commentId, otherUserId));
    }

    [Fact]
    public async Task DeleteAsync_WrongTask_ThrowsInvalidOperationException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var otherTaskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = otherTaskId,
            AuthorId = authorId,
            Body = "Body",
            CreatedAt = DateTime.UtcNow
        };

        _mockCommentRepo.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(taskId, commentId, authorId));
    }
}
