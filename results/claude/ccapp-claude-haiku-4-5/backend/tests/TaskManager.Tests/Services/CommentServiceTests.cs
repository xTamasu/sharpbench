using Moq;
using TaskManager.Application.Dto.Comment;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;
using DomainTask = TaskManager.Domain.Entities.Task;

namespace TaskManager.Tests.Services;

/// <summary>
/// Unit tests for CommentService.
/// </summary>
public class CommentServiceTests
{
    private readonly Mock<IRepository<TaskComment>> _commentRepositoryMock;
    private readonly Mock<IRepository<DomainTask>> _taskRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _commentRepositoryMock = new Mock<IRepository<TaskComment>>();
        _taskRepositoryMock = new Mock<IRepository<DomainTask>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _commentService = new CommentService(_commentRepositoryMock.Object, _taskRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskCommentsAsync_WithValidTaskId_ReturnsComments()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new DomainTask { Id = taskId };
        var user = new User { Id = userId, DisplayName = "Test User" };
        var comments = new List<TaskComment>
        {
            new TaskComment { Id = Guid.NewGuid(), TaskId = taskId, AuthorId = userId, Body = "Comment 1", Author = user }
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        _commentRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(comments);

        // Act
        var result = await _commentService.GetTaskCommentsAsync(taskId);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskCommentsAsync_WithInvalidTaskId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((DomainTask?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _commentService.GetTaskCommentsAsync(taskId));
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateCommentAsync_WithValidData_CreatesComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new DomainTask { Id = taskId };
        var request = new CreateCommentRequest { Body = "New comment" };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        _commentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TaskComment>())).ReturnsAsync((TaskComment c) => c);
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _commentService.CreateCommentAsync(taskId, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New comment", result.Body);
        Assert.Equal(userId, result.AuthorId);
        _commentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskComment>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateCommentAsync_WithCorrectAuthor_UpdatesComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Test User" };
        var comment = new TaskComment { Id = commentId, TaskId = taskId, AuthorId = userId, Body = "Old comment", Author = user };
        var request = new UpdateCommentRequest { Body = "Updated comment" };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);
        _commentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskComment>())).Returns(System.Threading.Tasks.Task.CompletedTask);
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _commentService.UpdateCommentAsync(taskId, commentId, request, userId);

        // Assert
        Assert.Equal("Updated comment", result.Body);
        Assert.NotNull(result.EditedAt);
        _commentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskComment>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateCommentAsync_WithWrongAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var user = new User { Id = authorId, DisplayName = "Test User" };
        var comment = new TaskComment { Id = commentId, TaskId = taskId, AuthorId = authorId, Body = "Comment", Author = user };
        var request = new UpdateCommentRequest { Body = "Updated" };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _commentService.UpdateCommentAsync(taskId, commentId, request, otherUserId));
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteCommentAsync_WithCorrectAuthor_DeletesComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, DisplayName = "Test User" };
        var comment = new TaskComment { Id = commentId, TaskId = taskId, AuthorId = userId, Author = user };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);
        _commentRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<TaskComment>())).Returns(System.Threading.Tasks.Task.CompletedTask);
        _commentRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _commentService.DeleteCommentAsync(taskId, commentId, userId);

        // Assert
        _commentRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<TaskComment>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteCommentAsync_WithWrongAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var user = new User { Id = authorId, DisplayName = "Test User" };
        var comment = new TaskComment { Id = commentId, TaskId = taskId, AuthorId = authorId, Author = user };

        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _commentService.DeleteCommentAsync(taskId, commentId, otherUserId));
    }
}
