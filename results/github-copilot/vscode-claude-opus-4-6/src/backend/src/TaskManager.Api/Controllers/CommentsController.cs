// Comments controller: handles task comment CRUD operations. All routes require JWT auth.
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

    // Extract the authenticated user's ID from the JWT sub claim
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> Create(Guid taskId, [FromBody] CreateCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await _commentService.CreateAsync(taskId, request, userId);
        return CreatedAtAction(nameof(Create), new { taskId, id = comment.Id }, comment);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> Update(Guid taskId, Guid id, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await _commentService.UpdateAsync(taskId, id, request, userId);
        return Ok(comment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid taskId, Guid id)
    {
        var userId = GetUserId();
        await _commentService.DeleteAsync(taskId, id, userId);
        return NoContent();
    }
}
