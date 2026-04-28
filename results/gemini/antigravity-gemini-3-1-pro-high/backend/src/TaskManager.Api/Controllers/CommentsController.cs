using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/{taskId}/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid taskId, CreateCommentRequest request)
        {
            var comment = await _commentService.AddCommentAsync(taskId, UserId, request);
            return Ok(comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditComment(Guid taskId, Guid id, UpdateCommentRequest request)
        {
            var comment = await _commentService.EditCommentAsync(taskId, id, UserId, request);
            return Ok(comment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(Guid taskId, Guid id)
        {
            await _commentService.DeleteCommentAsync(taskId, id, UserId);
            return NoContent();
        }
    }
}
