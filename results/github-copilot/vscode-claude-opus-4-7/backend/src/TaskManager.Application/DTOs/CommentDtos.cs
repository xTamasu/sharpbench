// CommentDtos.cs
// Request/response DTOs for the comment endpoints.
namespace TaskManager.Application.DTOs;

public record CreateCommentRequest(string Body);

public record UpdateCommentRequest(string Body);

public record CommentDto(
    Guid Id,
    Guid TaskId,
    UserDto Author,
    string Body,
    DateTime CreatedAt,
    DateTime? EditedAt);
