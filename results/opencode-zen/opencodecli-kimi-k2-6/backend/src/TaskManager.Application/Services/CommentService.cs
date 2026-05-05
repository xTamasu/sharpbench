using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

/// <summary>
/// Service for managing task comments, including creation, updates, and deletion.
/// </summary>
public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CommentResponse> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify task exists
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID '{taskId}' was not found.");
        }

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            AuthorId = userId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<TaskComment>().AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload to get author info
        var createdComment = await _unitOfWork.Repository<TaskComment>().GetByIdAsync(comment.Id, cancellationToken);
        return MapToResponse(createdComment!);
    }

    public async Task<CommentResponse> UpdateAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.Repository<TaskComment>().GetByIdAsync(commentId, cancellationToken);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID '{commentId}' was not found.");
        }

        if (comment.TaskId != taskId)
        {
            throw new KeyNotFoundException($"Comment with ID '{commentId}' does not belong to task '{taskId}'.");
        }

        // Only the comment author can edit their comment
        if (comment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("Only the comment author can edit this comment.");
        }

        comment.Body = request.Body;
        comment.EditedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<TaskComment>().UpdateAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedComment = await _unitOfWork.Repository<TaskComment>().GetByIdAsync(comment.Id, cancellationToken);
        return MapToResponse(updatedComment!);
    }

    public async Task DeleteAsync(Guid taskId, Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.Repository<TaskComment>().GetByIdAsync(commentId, cancellationToken);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID '{commentId}' was not found.");
        }

        if (comment.TaskId != taskId)
        {
            throw new KeyNotFoundException($"Comment with ID '{commentId}' does not belong to task '{taskId}'.");
        }

        // Only the comment author can delete their comment
        if (comment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("Only the comment author can delete this comment.");
        }

        await _unitOfWork.Repository<TaskComment>().DeleteAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static CommentResponse MapToResponse(TaskComment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Author?.DisplayName ?? string.Empty,
            Body = comment.Body,
            EditedAt = comment.EditedAt,
            CreatedAt = comment.CreatedAt
        };
    }
}
