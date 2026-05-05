// TasksController — CRUD endpoints for tasks with authorization and filtering support.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(idClaim.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] TaskStatusEnum? status,
        [FromQuery] TaskPriorityEnum? priority,
        [FromQuery] Guid? assignedToId,
        CancellationToken ct)
    {
        var filter = new TaskListFilter
        {
            Status = status,
            Priority = priority,
            AssignedToId = assignedToId
        };
        var tasks = await _taskService.GetAllAsync(filter, ct);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var task = await _taskService.GetByIdAsync(id, ct);
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.UpdateAsync(id, userId, request, ct);
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _taskService.DeleteAsync(id, userId, ct);
        return NoContent();
    }
}
