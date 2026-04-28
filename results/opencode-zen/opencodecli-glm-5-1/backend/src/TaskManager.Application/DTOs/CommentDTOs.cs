// Data transfer objects for comment operations
namespace TaskManager.Application.DTOs;

public record CreateCommentRequest(string Body);

public record UpdateCommentRequest(string Body);

public record CommentResponse(
    Guid Id,
    Guid TaskId,
    Guid AuthorId,
    string AuthorName,
    string Body,
    DateTime? EditedAt,
    DateTime CreatedAt);