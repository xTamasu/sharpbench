// CommentsController — endpoints for creating, editing, and deleting task comments.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Application.DTOs.Comments;
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

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(idClaim.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid taskId, CancellationToken ct)
    {
        var comments = await _commentService.GetByTaskIdAsync(taskId, ct);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid taskId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.CreateAsync(taskId, userId, request, ct);
        return Created(string.Empty, comment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid taskId, Guid id, [FromBody] UpdateCommentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.UpdateAsync(taskId, id, userId, request, ct);
        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _commentService.DeleteAsync(taskId, id, userId, ct);
        return NoContent();
    }
}
