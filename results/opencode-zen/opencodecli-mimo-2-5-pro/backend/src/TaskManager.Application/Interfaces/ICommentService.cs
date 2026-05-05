// Service interface for task comment business logic.

using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponse> AddCommentAsync(Guid taskId, CreateCommentRequest request, Guid authorId);
    Task<CommentResponse> UpdateCommentAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId);
    Task DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId);
}
