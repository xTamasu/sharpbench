namespace TaskManager.Application.Dto.Comment;

/// <summary>
/// Request to update an existing comment.
/// </summary>
public class UpdateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}
