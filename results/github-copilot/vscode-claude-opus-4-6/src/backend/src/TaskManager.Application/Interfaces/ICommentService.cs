// Service interface for task comment operations.
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponse> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId);
    Task<CommentResponse> UpdateAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId);
    Task DeleteAsync(Guid taskId, Guid commentId, Guid userId);
}
