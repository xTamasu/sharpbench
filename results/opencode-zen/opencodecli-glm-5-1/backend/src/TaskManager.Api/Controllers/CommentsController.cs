// Comments controller for adding, editing, and deleting task comments
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");
        return Guid.Parse(userIdClaim!.Value);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> Create(Guid taskId, CreateCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.CreateAsync(taskId, request, userId);
        return Ok(comment);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> Update(Guid taskId, Guid id, UpdateCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.UpdateAsync(taskId, id, request, userId);
        return Ok(comment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id)
    {
        var userId = GetCurrentUserId();
        await _commentService.DeleteAsync(taskId, id, userId);
        return NoContent();
    }
}