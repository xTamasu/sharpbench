// Unit tests for CommentService covering add, edit, delete, and ownership checks.

using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly CommentService _commentService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();
    private readonly Guid _taskId = Guid.NewGuid();

    public CommentServiceTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _commentService = new CommentService(
            _commentRepositoryMock.Object,
            _taskRepositoryMock.Object);
    }

    [Fact]
    public async Task AddCommentAsync_TaskExists_ReturnsComment()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = _taskId,
            Title = "Test Task",
            CreatedById = _userId,
            CreatedBy = new User { Id = _userId, Email = "test@test.com", DisplayName = "Test", PasswordHash = "hash" }
        };
        var request = new CreateCommentRequest("Great comment!");

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(_taskId)).ReturnsAsync(task);
        _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskComment>()))
            .ReturnsAsync((TaskComment c) => c);

        // Act
        var result = await _commentService.AddCommentAsync(_taskId, request, _userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Great comment!", result.Body);
        _commentRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CreateCommentRequest("Comment");
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(_taskId)).ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _commentService.AddCommentAsync(_taskId, request, _userId));
    }

    [Fact]
    public async Task UpdateCommentAsync_OwnerEdits_Succeeds()
    {
        // Arrange
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = _taskId,
            AuthorId = _userId,
            Body = "Original",
            CreatedAt = DateTime.UtcNow
        };
        var request = new UpdateCommentRequest("Updated comment");

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        // Act
        var result = await _commentService.UpdateCommentAsync(_taskId, comment.Id, request, _userId);

        // Assert
        Assert.Equal("Updated comment", result.Body);
        Assert.NotNull(result.EditedAt);
        _commentRepositoryMock.Verify(r => r.UpdateAsync(comment), Times.Once);
        _commentRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCommentAsync_NonOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange: comment owned by _userId, but _otherUserId tries to edit
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = _taskId,
            AuthorId = _userId,
            Body = "Original",
            CreatedAt = DateTime.UtcNow
        };
        var request = new UpdateCommentRequest("Updated");

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _commentService.UpdateCommentAsync(_taskId, comment.Id, request, _otherUserId));
        Assert.Equal("Only the comment author can edit this comment.", exception.Message);
    }

    [Fact]
    public async Task UpdateCommentAsync_WrongTaskId_ThrowsKeyNotFoundException()
    {
        // Arrange: comment belongs to _taskId, but we query with a different task ID
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = _taskId,
            AuthorId = _userId,
            Body = "Original",
            CreatedAt = DateTime.UtcNow
        };
        var wrongTaskId = Guid.NewGuid();
        var request = new UpdateCommentRequest("Updated");

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _commentService.UpdateCommentAsync(wrongTaskId, comment.Id, request, _userId));
    }

    [Fact]
    public async Task UpdateCommentAsync_CommentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateCommentRequest("Updated");
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(nonExistentId))
            .ReturnsAsync((TaskComment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _commentService.UpdateCommentAsync(_taskId, nonExistentId, request, _userId));
    }

    [Fact]
    public async Task DeleteCommentAsync_OwnerDeletes_Succeeds()
    {
        // Arrange
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = _taskId,
            AuthorId = _userId,
            Body = "To delete",
            CreatedAt = DateTime.UtcNow
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        // Act
        await _commentService.DeleteCommentAsync(_taskId, comment.Id, _userId);

        // Assert
        _commentRepositoryMock.Verify(r => r.DeleteAsync(comment), Times.Once);
        _commentRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_NonOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange: comment owned by _userId, but _otherUserId tries to delete
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = _taskId,
            AuthorId = _userId,
            Body = "To delete",
            CreatedAt = DateTime.UtcNow
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _commentService.DeleteCommentAsync(_taskId, comment.Id, _otherUserId));
        Assert.Equal("Only the comment author can delete this comment.", exception.Message);
    }

    [Fact]
    public async Task DeleteCommentAsync_WrongTaskId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = _taskId,
            AuthorId = _userId,
            Body = "To delete",
            CreatedAt = DateTime.UtcNow
        };
        var wrongTaskId = Guid.NewGuid();

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _commentService.DeleteCommentAsync(wrongTaskId, comment.Id, _userId));
    }

    [Fact]
    public async Task DeleteCommentAsync_CommentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(nonExistentId))
            .ReturnsAsync((TaskComment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _commentService.DeleteCommentAsync(_taskId, nonExistentId, _userId));
    }
}
