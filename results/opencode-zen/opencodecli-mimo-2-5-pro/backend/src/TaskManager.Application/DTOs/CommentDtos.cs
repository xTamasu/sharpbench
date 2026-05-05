// DTOs for task comment operations (create, update, response).

namespace TaskManager.Application.DTOs;

public record CreateCommentRequest(string Body);

public record UpdateCommentRequest(string Body);

public record CommentResponse(
    Guid Id,
    Guid TaskId,
    UserDto Author,
    string Body,
    DateTime? EditedAt,
    DateTime CreatedAt);
