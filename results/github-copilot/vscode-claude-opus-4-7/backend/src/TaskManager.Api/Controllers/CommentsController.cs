// CommentsController.cs
// Manages comments under a given task. All endpoints require authentication.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Auth;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks/{taskId:guid}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _comments;
    private readonly ICurrentUser _currentUser;

    public CommentsController(ICommentService comments, ICurrentUser currentUser)
    {
        _comments = comments;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> Add(Guid taskId, [FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        var dto = await _comments.AddAsync(taskId, req, _currentUser.Id, ct);
        return Created($"/api/tasks/{taskId}/comments/{dto.Id}", dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentDto>> Update(Guid taskId, Guid id, [FromBody] UpdateCommentRequest req, CancellationToken ct)
        => Ok(await _comments.UpdateAsync(taskId, id, req, _currentUser.Id, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id, CancellationToken ct)
    {
        await _comments.DeleteAsync(taskId, id, _currentUser.Id, ct);
        return NoContent();
    }
}
