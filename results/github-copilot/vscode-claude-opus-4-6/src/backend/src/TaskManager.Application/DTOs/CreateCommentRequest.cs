// DTO for creating a new comment on a task.
namespace TaskManager.Application.DTOs;

public class CreateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}
