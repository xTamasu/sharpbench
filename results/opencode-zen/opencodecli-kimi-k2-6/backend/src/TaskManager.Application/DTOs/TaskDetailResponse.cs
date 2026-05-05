namespace TaskManager.Application.DTOs;

/// <summary>
/// DTO for task detail response including comments.
/// </summary>
public class TaskDetailResponse : TaskResponse
{
    public List<CommentResponse> Comments { get; set; } = new();
}
