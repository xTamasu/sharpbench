// CommentService.cs
// Implements add/edit/delete for comments. Edit and delete are restricted
// to the original author. Editing sets EditedAt to the current UTC time.
using FluentValidation;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Mapping;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _comments;
    private readonly ITaskRepository _tasks;
    private readonly IUserRepository _users;
    private readonly IValidator<CreateCommentRequest> _createValidator;
    private readonly IValidator<UpdateCommentRequest> _updateValidator;

    public CommentService(
        ICommentRepository comments,
        ITaskRepository tasks,
        IUserRepository users,
        IValidator<CreateCommentRequest> createValidator,
        IValidator<UpdateCommentRequest> updateValidator)
    {
        _comments = comments;
        _tasks = tasks;
        _users = users;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<CommentDto> AddAsync(Guid taskId, CreateCommentRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationFailedException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var task = await _tasks.GetByIdAsync(taskId, ct)
            ?? throw new NotFoundException($"Task {taskId} not found.");

        var author = await _users.GetByIdAsync(currentUserId, ct)
            ?? throw new NotFoundException("Current user not found.");

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            AuthorId = currentUserId,
            Author = author,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _comments.AddAsync(comment, ct);
        await _comments.SaveChangesAsync(ct);

        return comment.ToDto();
    }

    public async Task<CommentDto> UpdateAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationFailedException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var comment = await _comments.GetByIdAsync(commentId, ct)
            ?? throw new NotFoundException($"Comment {commentId} not found.");

        if (comment.TaskId != taskId)
            throw new NotFoundException($"Comment {commentId} does not belong to task {taskId}.");

        // Ownership: only the author may edit.
        if (comment.AuthorId != currentUserId)
            throw new ForbiddenException("Only the author can edit this comment.");

        comment.Body = request.Body.Trim();
        comment.EditedAt = DateTime.UtcNow;

        _comments.Update(comment);
        await _comments.SaveChangesAsync(ct);

        // Re-load author for the response if it was not tracked.
        comment.Author ??= await _users.GetByIdAsync(comment.AuthorId, ct);
        return comment.ToDto();
    }

    public async Task DeleteAsync(Guid taskId, Guid commentId, Guid currentUserId, CancellationToken ct = default)
    {
        var comment = await _comments.GetByIdAsync(commentId, ct)
            ?? throw new NotFoundException($"Comment {commentId} not found.");

        if (comment.TaskId != taskId)
            throw new NotFoundException($"Comment {commentId} does not belong to task {taskId}.");

        if (comment.AuthorId != currentUserId)
            throw new ForbiddenException("Only the author can delete this comment.");

        _comments.Remove(comment);
        await _comments.SaveChangesAsync(ct);
    }
}
