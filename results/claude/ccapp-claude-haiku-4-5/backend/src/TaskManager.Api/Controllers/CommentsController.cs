using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Dto.Comment;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers;

/// <summary>
/// Comments controller for managing task comments.
/// </summary>
[ApiController]
[Route("api/tasks/{taskId}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetTaskComments([FromRoute] Guid taskId)
    {
        var comments = await _commentService.GetTaskCommentsAsync(taskId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromRoute] Guid taskId, [FromBody] CreateCommentRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
        var comment = await _commentService.CreateCommentAsync(taskId, request, userId);
        return CreatedAtAction(nameof(GetTaskComments), new { taskId }, comment);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CommentDto>> UpdateComment([FromRoute] Guid taskId, [FromRoute] Guid id, [FromBody] UpdateCommentRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
        var comment = await _commentService.UpdateCommentAsync(taskId, id, request, userId);
        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment([FromRoute] Guid taskId, [FromRoute] Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
        await _commentService.DeleteCommentAsync(taskId, id, userId);
        return NoContent();
    }
}
