// TasksController.cs
// CRUD for tasks. All endpoints require an authenticated user (JWT bearer).
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Auth;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;
    private readonly ICurrentUser _currentUser;

    public TasksController(ITaskService tasks, ICurrentUser currentUser)
    {
        _tasks = tasks;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> List(
        [FromQuery] TaskItemStatus? status,
        [FromQuery] TaskPriority? priority,
        [FromQuery] Guid? assignedToId,
        CancellationToken ct)
        => Ok(await _tasks.ListAsync(status, priority, assignedToId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDetailDto>> Get(Guid id, CancellationToken ct)
        => Ok(await _tasks.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest req, CancellationToken ct)
    {
        var created = await _tasks.CreateAsync(req, _currentUser.Id, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskRequest req, CancellationToken ct)
        => Ok(await _tasks.UpdateAsync(id, req, _currentUser.Id, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _tasks.DeleteAsync(id, _currentUser.Id, ct);
        return NoContent();
    }
}
