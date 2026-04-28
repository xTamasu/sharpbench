using TaskManager.Application.Dto.Comment;

namespace TaskManager.Application.Services;

/// <summary>
/// Comment service interface.
/// </summary>
public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetTaskCommentsAsync(Guid taskId);
    Task<CommentDto> CreateCommentAsync(Guid taskId, CreateCommentRequest request, Guid userId);
    Task<CommentDto> UpdateCommentAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId);
    Task DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId);
}
