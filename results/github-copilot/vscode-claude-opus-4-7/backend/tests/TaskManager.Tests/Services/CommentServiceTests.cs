// CommentServiceTests.cs
// Unit tests for CommentService covering add/edit/delete with ownership rules.
using FluentValidation;
using Moq;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Application.Validation;
using TaskManager.Domain.Entities;

namespace TaskManager.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly IValidator<CreateCommentRequest> _createV = new CreateCommentRequestValidator();
    private readonly IValidator<UpdateCommentRequest> _updateV = new UpdateCommentRequestValidator();

    private CommentService Build() => new(_comments.Object, _tasks.Object, _users.Object, _createV, _updateV);

    [Fact]
    public async Task Add_HappyPath_ReturnsDtoAndPersists()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid() };
        var author = new User { Id = Guid.NewGuid(), Email = "a@b.c", DisplayName = "A", PasswordHash = "h" };
        _tasks.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _users.Setup(r => r.GetByIdAsync(author.Id, It.IsAny<CancellationToken>())).ReturnsAsync(author);

        // Act
        var dto = await Build().AddAsync(task.Id, new CreateCommentRequest("hello"), author.Id);

        // Assert
        Assert.Equal("hello", dto.Body);
        Assert.Equal(author.Id, dto.Author.Id);
        _comments.Verify(r => r.AddAsync(It.IsAny<TaskComment>(), It.IsAny<CancellationToken>()), Times.Once);
        _comments.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Add_EmptyBody_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationFailedException>(
            () => Build().AddAsync(Guid.NewGuid(), new CreateCommentRequest(""), Guid.NewGuid()));
    }

    [Fact]
    public async Task Add_BodyTooLong_ThrowsValidation()
    {
        var longBody = new string('x', 2001);
        await Assert.ThrowsAsync<ValidationFailedException>(
            () => Build().AddAsync(Guid.NewGuid(), new CreateCommentRequest(longBody), Guid.NewGuid()));
    }

    [Fact]
    public async Task Add_TaskMissing_ThrowsNotFound()
    {
        _tasks.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(
            () => Build().AddAsync(Guid.NewGuid(), new CreateCommentRequest("hi"), Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_OnlyAuthor_ThrowsForOthers()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var author = Guid.NewGuid();
        var attacker = Guid.NewGuid();
        var comment = new TaskComment { Id = Guid.NewGuid(), TaskId = taskId, AuthorId = author, Body = "old" };
        _comments.Setup(r => r.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => Build().UpdateAsync(taskId, comment.Id, new UpdateCommentRequest("new"), attacker));
    }

    [Fact]
    public async Task Update_Author_SetsEditedAt()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var author = new User { Id = Guid.NewGuid(), Email = "a@b.c", DisplayName = "A", PasswordHash = "h" };
        var comment = new TaskComment { Id = Guid.NewGuid(), TaskId = taskId, AuthorId = author.Id, Author = author, Body = "old" };
        _comments.Setup(r => r.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
        _users.Setup(r => r.GetByIdAsync(author.Id, It.IsAny<CancellationToken>())).ReturnsAsync(author);

        // Act
        var dto = await Build().UpdateAsync(taskId, comment.Id, new UpdateCommentRequest("new"), author.Id);

        // Assert
        Assert.Equal("new", comment.Body);
        Assert.NotNull(comment.EditedAt);
        Assert.Equal("new", dto.Body);
    }

    [Fact]
    public async Task Update_WrongTask_ThrowsNotFound()
    {
        var comment = new TaskComment { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), AuthorId = Guid.NewGuid() };
        _comments.Setup(r => r.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        await Assert.ThrowsAsync<NotFoundException>(
            () => Build().UpdateAsync(Guid.NewGuid() /* different */, comment.Id, new UpdateCommentRequest("x"), comment.AuthorId));
    }

    [Fact]
    public async Task Update_Missing_ThrowsNotFound()
    {
        _comments.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskComment?)null);
        await Assert.ThrowsAsync<NotFoundException>(
            () => Build().UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateCommentRequest("x"), Guid.NewGuid()));
    }

    [Fact]
    public async Task Delete_OnlyAuthor_ThrowsForOthers()
    {
        var comment = new TaskComment { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), AuthorId = Guid.NewGuid() };
        _comments.Setup(r => r.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => Build().DeleteAsync(comment.TaskId, comment.Id, Guid.NewGuid()));
        _comments.Verify(r => r.Remove(It.IsAny<TaskComment>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Author_Removes()
    {
        var comment = new TaskComment { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), AuthorId = Guid.NewGuid() };
        _comments.Setup(r => r.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        await Build().DeleteAsync(comment.TaskId, comment.Id, comment.AuthorId);

        _comments.Verify(r => r.Remove(comment), Times.Once);
        _comments.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Missing_ThrowsNotFound()
    {
        _comments.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskComment?)null);
        await Assert.ThrowsAsync<NotFoundException>(
            () => Build().DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
    }
}
