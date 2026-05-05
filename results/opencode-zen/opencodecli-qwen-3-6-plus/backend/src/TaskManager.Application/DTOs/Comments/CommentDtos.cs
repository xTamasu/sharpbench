// Comment DTOs — request and response shapes for comment endpoints.

using System;

namespace TaskManager.Application.DTOs.Comments;

public class CreateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}

public class CommentResponse
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
