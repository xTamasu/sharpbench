namespace TaskManager.Application.DTOs;

/// <summary>
/// DTO for updating an existing comment.
/// </summary>
public class UpdateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}
