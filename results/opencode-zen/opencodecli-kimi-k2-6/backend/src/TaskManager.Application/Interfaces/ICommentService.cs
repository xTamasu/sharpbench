using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Service interface for comment management operations.
/// </summary>
public interface ICommentService
{
    Task<CommentResponse> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentResponse> UpdateAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid taskId, Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
