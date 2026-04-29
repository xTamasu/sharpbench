// ICommentService.cs
// Service contract for managing comments on a given task.
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Services;

public interface ICommentService
{
    Task<CommentDto> AddAsync(Guid taskId, CreateCommentRequest request, Guid currentUserId, CancellationToken ct = default);
    Task<CommentDto> UpdateAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid currentUserId, CancellationToken ct = default);
    Task DeleteAsync(Guid taskId, Guid commentId, Guid currentUserId, CancellationToken ct = default);
}
