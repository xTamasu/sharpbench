using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.Services;

/// <summary>
/// Unit tests for the CommentService covering creation, updates, deletion, and ownership rules.
/// </summary>
public class CommentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<TaskItem>> _taskRepositoryMock;
    private readonly Mock<IRepository<TaskComment>> _commentRepositoryMock;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<IRepository<TaskItem>>();
        _commentRepositoryMock = new Mock<IRepository<TaskComment>>();

        _unitOfWorkMock.Setup(u => u.Repository<TaskItem>()).Returns(_taskRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<TaskComment>()).Returns(_commentRepositoryMock.Object);

        _commentService = new CommentService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest { Body = "This is a comment." };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskItem { Id = taskId });

        _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskComment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskComment c, CancellationToken _) => c);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new TaskComment
            {
                Id = id,
                TaskId = taskId,
                AuthorId = userId,
                Body = request.Body,
                Author = new User { DisplayName = "Test User" }
            });

        // Act
        var result = await _commentService.CreateAsync(taskId, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Body, result.Body);
        Assert.Equal(userId, result.AuthorId);
    }

    [Fact]
    public async Task CreateAsync_NonExistentTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest { Body = "This is a comment." };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _commentService.CreateAsync(taskId, request, userId));
        Assert.Contains(taskId.ToString(), exception.Message);
    }

    [Fact]
    public async Task CreateAsync_EmptyBody_ThrowsValidationExceptionOnController()
    {
        // Note: Empty body validation is handled by FluentValidation in the controller.
        // This test verifies the service behavior with empty body.
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest { Body = "" };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskItem { Id = taskId });

        _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskComment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskComment c, CancellationToken _) => c);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new TaskComment
            {
                Id = id,
                TaskId = taskId,
                AuthorId = userId,
                Body = request.Body
            });

        // Act
        var result = await _commentService.CreateAsync(taskId, request, userId);

        // Assert - service allows empty body, controller validation catches it
        Assert.NotNull(result);
        Assert.Empty(result.Body);
    }

    [Fact]
    public async Task UpdateAsync_Author_ReturnsUpdatedComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Body = "Updated comment body." };

        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Original body"
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act
        var result = await _commentService.UpdateAsync(taskId, commentId, request, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Body, result.Body);
    }

    [Fact]
    public async Task UpdateAsync_SetsEditedAt()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Body = "Updated comment body." };

        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Original body"
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act
        var result = await _commentService.UpdateAsync(taskId, commentId, request, authorId);

        // Assert
        _commentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<TaskComment>(c =>
            c.EditedAt.HasValue &&
            c.EditedAt.Value <= DateTime.UtcNow &&
            c.EditedAt.Value > DateTime.UtcNow.AddMinutes(-1)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Body = "Updated comment body." };

        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Original body"
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _commentService.UpdateAsync(taskId, commentId, request, otherUserId));
        Assert.Equal("Only the comment author can edit this comment.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WrongTask_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var wrongTaskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Body = "Updated comment body." };

        var comment = new TaskComment
        {
            Id = commentId,
            TaskId = taskId,
            AuthorId = authorId,
            Body = "Original body"
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _commentService.UpdateAsync(wrongTaskId, commentId, request, authorId));
        Assert.Contains(wrongTaskId.ToString(), exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentComment_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Body = "Updated comment body." };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskComment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _commentService.UpdateAsync(taskId, commentId, request, authorId));
        Assert.Contains(commentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_Author_DeletesComment()
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
            Body = "Comment to delete"
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _commentService.DeleteAsync(taskId, commentId, authorId);

        // Assert
        _commentRepositoryMock.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotAuthor_ThrowsUnauthorizedAccessException()
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
            Body = "Comment to delete"
        };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _commentService.DeleteAsync(taskId, commentId, otherUserId));
        Assert.Equal("Only the comment author can delete this comment.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentComment_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskComment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _commentService.DeleteAsync(taskId, commentId, userId));
        Assert.Contains(commentId.ToString(), exception.Message);
    }
}
