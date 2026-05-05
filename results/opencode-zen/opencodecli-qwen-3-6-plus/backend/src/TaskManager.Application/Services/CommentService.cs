// CommentService — business logic for comments with author-based edit/delete enforcement.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Comments;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class CommentService : ICommentService
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<TaskComment> _commentRepository;
    private readonly IRepository<User> _userRepository;

    public CommentService(
        IRepository<TaskItem> taskRepository,
        IRepository<TaskComment> commentRepository,
        IRepository<User> userRepository)
    {
        _taskRepository = taskRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<CommentResponse>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        var commentsQuery = await _commentRepository.GetAllAsync(ct);
        var comments = commentsQuery
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToList();

        return comments.Select(MapToResponse);
    }

    public async Task<CommentResponse> CreateAsync(Guid taskId, Guid authorId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id {taskId} not found.");
        }

        var author = await _userRepository.GetByIdAsync(authorId, ct);
        if (author == null)
        {
            throw new KeyNotFoundException($"User with id {authorId} not found.");
        }

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            AuthorId = authorId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, ct);
        comment.Author = author;
        comment.Task = task;

        return MapToResponse(comment);
    }

    public async Task<CommentResponse> UpdateAsync(Guid taskId, Guid commentId, Guid authorId, UpdateCommentRequest request, CancellationToken ct = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, ct);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with id {commentId} not found.");
        }

        if (comment.TaskId != taskId)
        {
            throw new InvalidOperationException("Comment does not belong to the specified task.");
        }

        // Only the comment author can edit
        if (comment.AuthorId != authorId)
        {
            throw new UnauthorizedAccessException("Only the comment author can edit a comment.");
        }

        comment.Body = request.Body;
        comment.EditedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment, ct);

        var author = await _userRepository.GetByIdAsync(authorId, ct);
        if (author != null)
        {
            comment.Author = author;
        }

        return MapToResponse(comment);
    }

    public async Task DeleteAsync(Guid taskId, Guid commentId, Guid authorId, CancellationToken ct = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, ct);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with id {commentId} not found.");
        }

        if (comment.TaskId != taskId)
        {
            throw new InvalidOperationException("Comment does not belong to the specified task.");
        }

        // Only the comment author can delete
        if (comment.AuthorId != authorId)
        {
            throw new UnauthorizedAccessException("Only the comment author can delete a comment.");
        }

        await _commentRepository.DeleteAsync(comment, ct);
    }

    private static CommentResponse MapToResponse(TaskComment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Task?.CreatedBy?.DisplayName ?? comment.Author?.DisplayName ?? string.Empty,
            Body = comment.Body,
            EditedAt = comment.EditedAt,
            CreatedAt = comment.CreatedAt
        };
    }
}
