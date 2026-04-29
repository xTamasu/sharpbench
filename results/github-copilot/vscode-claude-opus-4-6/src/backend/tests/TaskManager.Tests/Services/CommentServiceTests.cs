// Unit tests for CommentService: covers create, update, delete, ownership, and not-found.
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ITaskCommentRepository> _commentRepoMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _commentRepoMock = new Mock<ITaskCommentRepository>();
        _taskRepoMock = new Mock<ITaskRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new CommentService(_commentRepoMock.Object, _taskRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_HappyPath_ReturnsComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest { Body = "Great work!" };

        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync(new TaskItem { Id = taskId });
        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, DisplayName = "Author" });

        // Act
        var result = await _sut.CreateAsync(taskId, request, userId);

        // Assert
        Assert.Equal("Great work!", result.Body);
        Assert.Equal(taskId, result.TaskId);
        Assert.Equal(userId, result.AuthorId);
        Assert.Equal("Author", result.AuthorName);
        _commentRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskComment>()), Times.Once);
        _commentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TaskNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var request = new CreateCommentRequest { Body = "Comment" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.CreateAsync(taskId, request, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_HappyPath_Author_ReturnsUpdatedComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingComment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = userId,
            Body = "Original",
            CreatedAt = DateTime.UtcNow
        };
        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(existingComment);
        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, DisplayName = "Author" });

        var request = new UpdateCommentRequest { Body = "Updated body" };

        // Act
        var result = await _sut.UpdateAsync(taskId, commentId, request, userId);

        // Assert — EditedAt should be set on edit
        Assert.Equal("Updated body", result.Body);
        Assert.NotNull(result.EditedAt);
        _commentRepoMock.Verify(r => r.Update(It.IsAny<TaskComment>()), Times.Once);
        _commentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var existingComment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Original"
        };
        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(existingComment);

        var request = new UpdateCommentRequest { Body = "Hacked!" };

        // Act & Assert — only the author can edit their comment
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.UpdateAsync(taskId, commentId, request, otherUserId));
    }

    [Fact]
    public async Task UpdateAsync_CommentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync((TaskComment?)null);

        var request = new UpdateCommentRequest { Body = "Updated" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateAsync(taskId, commentId, request, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_CommentOnWrongTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var wrongTaskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var existingComment = new TaskComment
        {
            Id = commentId,
            TaskId = wrongTaskId, // comment belongs to a different task
            AuthorId = Guid.NewGuid(),
            Body = "Original"
        };
        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(existingComment);

        var request = new UpdateCommentRequest { Body = "Updated" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateAsync(taskId, commentId, request, existingComment.AuthorId));
    }

    [Fact]
    public async Task DeleteAsync_HappyPath_Author_DeletesComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingComment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = userId,
            Body = "To delete"
        };
        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(existingComment);

        // Act
        await _sut.DeleteAsync(taskId, commentId, userId);

        // Assert
        _commentRepoMock.Verify(r => r.Remove(existingComment), Times.Once);
        _commentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var existingComment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Not yours"
        };
        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(existingComment);

        // Act & Assert — only the author can delete their comment
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.DeleteAsync(taskId, commentId, otherUserId));
    }

    [Fact]
    public async Task DeleteAsync_CommentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        _commentRepoMock.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync((TaskComment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.DeleteAsync(taskId, commentId, Guid.NewGuid()));
    }
}
