// Comment service implementation with ownership checks for edit/delete.

using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;

    public CommentService(ICommentRepository commentRepository, ITaskRepository taskRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
    }

    public async Task<CommentResponse> AddCommentAsync(
        Guid taskId, CreateCommentRequest request, Guid authorId)
    {
        // Verify the task exists
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            AuthorId = authorId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        return new CommentResponse(
            comment.Id,
            comment.TaskId,
            new UserDto(comment.AuthorId, "", ""),
            comment.Body,
            comment.EditedAt,
            comment.CreatedAt);
    }

    public async Task<CommentResponse> UpdateCommentAsync(
        Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId)
            ?? throw new KeyNotFoundException($"Comment with ID {commentId} not found.");

        // Verify the comment belongs to the specified task
        if (comment.TaskId != taskId)
            throw new KeyNotFoundException($"Comment with ID {commentId} not found on task {taskId}.");

        // Only the comment author can edit
        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Only the comment author can edit this comment.");

        comment.Body = request.Body;
        comment.EditedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);
        await _commentRepository.SaveChangesAsync();

        return new CommentResponse(
            comment.Id,
            comment.TaskId,
            new UserDto(comment.AuthorId, "", ""),
            comment.Body,
            comment.EditedAt,
            comment.CreatedAt);
    }

    public async Task DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId)
            ?? throw new KeyNotFoundException($"Comment with ID {commentId} not found.");

        // Verify the comment belongs to the specified task
        if (comment.TaskId != taskId)
            throw new KeyNotFoundException($"Comment with ID {commentId} not found on task {taskId}.");

        // Only the comment author can delete
        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Only the comment author can delete this comment.");

        await _commentRepository.DeleteAsync(comment);
        await _commentRepository.SaveChangesAsync();
    }
}
