namespace TaskManager.Application.DTOs;

/// <summary>
/// DTO for creating a new comment on a task.
/// </summary>
public class CreateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}
