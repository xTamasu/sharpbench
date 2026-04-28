// Unit tests for CommentService covering CRUD and ownership checks
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Tests;

public class CommentServiceTests
{
    private readonly Mock<ITaskCommentRepository> _commentRepoMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _commentRepoMock = new Mock<ITaskCommentRepository>();
        _taskRepoMock = new Mock<ITaskRepository>();
        _sut = new CommentService(_commentRepoMock.Object, _taskRepoMock.Object);
    }

    private static TaskComment CreateSampleComment(Guid? authorId = null)
    {
        var id = authorId ?? Guid.NewGuid();
        return new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            AuthorId = id,
            Author = new User { Id = id, DisplayName = "Author" },
            Body = "Sample comment",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new DomainTask { Id = taskId };
        var comment = CreateSampleComment(userId);
        comment.TaskId = taskId;

        _taskRepoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
        _commentRepoMock.Setup(r => r.AddAsync(It.IsAny<TaskComment>())).ReturnsAsync((TaskComment c) => c);
        _commentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(comment);

        var request = new CreateCommentRequest("A new comment");

        // Act
        var result = await _sut.CreateAsync(taskId, request, userId);

        // Assert
        _commentRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskComment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DomainTask?)null);
        var request = new CreateCommentRequest("A comment");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(Guid.NewGuid(), request, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_AuthorEdits_Succeeds()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = CreateSampleComment(authorId);
        var updatedComment = CreateSampleComment(authorId);
        updatedComment.Body = "Updated body";
        updatedComment.EditedAt = DateTime.UtcNow;

        // GetByIdAsync is called twice: first to fetch the comment, then to re-fetch after update
        _commentRepoMock.SetupSequence(r => r.GetByIdAsync(comment.Id))
            .ReturnsAsync(comment)
            .ReturnsAsync(updatedComment);
        _commentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskComment>())).Returns(Task.CompletedTask);

        var request = new UpdateCommentRequest("Updated body");

        // Act
        var result = await _sut.UpdateAsync(comment.TaskId, comment.Id, request, authorId);

        // Assert
        _commentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TaskComment>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonAuthorEdits_ThrowsForbiddenException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var comment = CreateSampleComment(authorId);

        _commentRepoMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        var request = new UpdateCommentRequest("Updated body");

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.UpdateAsync(comment.TaskId, comment.Id, request, otherUserId));
    }

    [Fact]
    public async Task UpdateAsync_NonExistentComment_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskComment?)null);
        var request = new UpdateCommentRequest("Updated body");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), request, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_SetsEditedAt()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = CreateSampleComment(authorId);
        comment.EditedAt = null;

        var capturedComment = comment;
        _commentRepoMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(() => capturedComment);
        _commentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskComment>()))
            .Callback<TaskComment>(c => { capturedComment = c; })
            .Returns(Task.CompletedTask);

        var request = new UpdateCommentRequest("Updated body");

        // Act
        await _sut.UpdateAsync(comment.TaskId, comment.Id, request, authorId);

        // Assert
        Assert.NotNull(capturedComment.EditedAt);
    }

    [Fact]
    public async Task DeleteAsync_AuthorDeletes_Succeeds()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = CreateSampleComment(authorId);

        _commentRepoMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);
        _commentRepoMock.Setup(r => r.DeleteAsync(It.IsAny<TaskComment>())).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(comment.TaskId, comment.Id, authorId);

        // Assert
        _commentRepoMock.Verify(r => r.DeleteAsync(It.IsAny<TaskComment>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonAuthorDeletes_ThrowsForbiddenException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var comment = CreateSampleComment(authorId);

        _commentRepoMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(comment.TaskId, comment.Id, otherUserId));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentComment_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskComment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WrongTaskId_ThrowsNotFoundException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = CreateSampleComment(authorId);
        var wrongTaskId = Guid.NewGuid();

        _commentRepoMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);
        var request = new UpdateCommentRequest("Updated body");

        // Act & Assert - comment's TaskId doesn't match the provided taskId
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(wrongTaskId, comment.Id, request, authorId));
    }
}