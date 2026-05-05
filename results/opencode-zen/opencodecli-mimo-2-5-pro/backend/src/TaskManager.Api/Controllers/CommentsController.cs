// Comments controller for task comment operations. Requires authentication.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId}/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> AddComment(
        Guid taskId, [FromBody] CreateCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await _commentService.AddCommentAsync(taskId, request, userId);
        return Created($"/api/tasks/{taskId}/comments/{comment.Id}", comment);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CommentResponse>> UpdateComment(
        Guid taskId, Guid id, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await _commentService.UpdateCommentAsync(taskId, id, request, userId);
        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid taskId, Guid id)
    {
        var userId = GetUserId();
        await _commentService.DeleteCommentAsync(taskId, id, userId);
        return NoContent();
    }

    private Guid GetUserId()
    {
        // Extract user ID from JWT claims
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(claim.Value);
    }
}
