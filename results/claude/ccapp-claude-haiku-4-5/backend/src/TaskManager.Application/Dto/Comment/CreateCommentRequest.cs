namespace TaskManager.Application.Dto.Comment;

/// <summary>
/// Request to create a new comment.
/// </summary>
public class CreateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}
