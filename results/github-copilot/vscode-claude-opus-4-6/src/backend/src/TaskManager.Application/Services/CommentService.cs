// Comment service: handles comment CRUD with ownership enforcement.
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class CommentService : ICommentService
{
    private readonly ITaskCommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(
        ITaskCommentRepository commentRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<CommentResponse> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId)
    {
        // Verify the task exists
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id '{taskId}' not found");
        }

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            AuthorId = userId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        // Fetch author name for response
        var author = await _userRepository.GetByIdAsync(userId);

        return new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = author?.DisplayName ?? string.Empty,
            Body = comment.Body,
            EditedAt = comment.EditedAt,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<CommentResponse> UpdateAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null || comment.TaskId != taskId)
        {
            throw new KeyNotFoundException($"Comment with id '{commentId}' not found on task '{taskId}'");
        }

        // Only the comment author can edit — ownership check
        if (comment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("Only the comment author can edit this comment");
        }

        // Update body and set EditedAt timestamp
        comment.Body = request.Body;
        comment.EditedAt = DateTime.UtcNow;

        _commentRepository.Update(comment);
        await _commentRepository.SaveChangesAsync();

        var author = await _userRepository.GetByIdAsync(userId);

        return new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = author?.DisplayName ?? string.Empty,
            Body = comment.Body,
            EditedAt = comment.EditedAt,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid taskId, Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null || comment.TaskId != taskId)
        {
            throw new KeyNotFoundException($"Comment with id '{commentId}' not found on task '{taskId}'");
        }

        // Only the comment author can delete — ownership check
        if (comment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("Only the comment author can delete this comment");
        }

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync();
    }
}
