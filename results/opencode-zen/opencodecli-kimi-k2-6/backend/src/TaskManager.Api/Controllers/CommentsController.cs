using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers;

/// <summary>
/// API controller for task comment operations.
/// </summary>
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

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> Create(
        Guid taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.CreateAsync(taskId, request, userId, cancellationToken);
        return Ok(comment);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> Update(
        Guid taskId,
        Guid id,
        UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.UpdateAsync(taskId, id, request, userId, cancellationToken);
        return Ok(comment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid taskId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _commentService.DeleteAsync(taskId, id, userId, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identifier in token.");
        }

        return userId;
    }
}
