// Comment service implementing comment operations with ownership validation
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class CommentService : ICommentService
{
    private readonly ITaskCommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;

    public CommentService(ITaskCommentRepository commentRepository, ITaskRepository taskRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
    }

    public async Task<CommentResponse> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId)
    {
        // Verify task exists
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new NotFoundException("Task not found.");

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            AuthorId = userId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);

        // Re-fetch to get navigation properties
        var created = await _commentRepository.GetByIdAsync(comment.Id);
        return new CommentResponse(
            created!.Id,
            created.TaskId,
            created.AuthorId,
            created.Author.DisplayName,
            created.Body,
            created.EditedAt,
            created.CreatedAt
        );
    }

    public async Task<CommentResponse> UpdateAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId)
    {
        // Editing a comment sets EditedAt to current UTC time
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new NotFoundException("Comment not found.");

        if (comment.TaskId != taskId)
            throw new NotFoundException("Comment not found on the specified task.");

        // Only the comment author can edit their comment
        if (comment.AuthorId != userId)
            throw new ForbiddenException("Only the comment author can edit this comment.");

        comment.Body = request.Body;
        comment.EditedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);

        var updated = await _commentRepository.GetByIdAsync(commentId);
        return new CommentResponse(
            updated!.Id,
            updated.TaskId,
            updated.AuthorId,
            updated.Author.DisplayName,
            updated.Body,
            updated.EditedAt,
            updated.CreatedAt
        );
    }

    public async Task DeleteAsync(Guid taskId, Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new NotFoundException("Comment not found.");

        if (comment.TaskId != taskId)
            throw new NotFoundException("Comment not found on the specified task.");

        // Only the comment author can delete their comment
        if (comment.AuthorId != userId)
            throw new ForbiddenException("Only the comment author can delete this comment.");

        await _commentRepository.DeleteAsync(comment);
    }
}