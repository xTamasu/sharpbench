// TaskService.cs
// Implements task CRUD with ownership and validation rules.
// - Only the creator may delete a task.
// - Any authenticated user may update task fields (no requirement said otherwise).
// - UpdatedAt is bumped on every modification.
using FluentValidation;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Mapping;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IUserRepository _users;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;

    public TaskService(
        ITaskRepository tasks,
        IUserRepository users,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator)
    {
        _tasks = tasks;
        _users = users;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<TaskDto>> ListAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? assignedToId,
        CancellationToken ct = default)
    {
        var items = await _tasks.QueryAsync(status, priority, assignedToId, ct);
        return items.Select(t => t.ToDto()).ToList();
    }

    public async Task<TaskDetailDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdWithCommentsAsync(id, ct)
            ?? throw new NotFoundException($"Task {id} not found.");
        return task.ToDetailDto();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationFailedException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        // Ensure the assignee (if provided) actually exists.
        if (request.AssignedToId is { } assignee && await _users.GetByIdAsync(assignee, ct) is null)
            throw new ValidationFailedException($"Assignee {assignee} does not exist.");

        var creator = await _users.GetByIdAsync(currentUserId, ct)
            ?? throw new NotFoundException("Current user not found.");

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId,
            CreatedById = currentUserId,
            CreatedBy = creator,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _tasks.AddAsync(task, ct);
        await _tasks.SaveChangesAsync(ct);

        // Re-fetch with navigations populated for a faithful response shape.
        var fresh = await _tasks.GetByIdWithCommentsAsync(task.Id, ct);
        return fresh!.ToDto();
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationFailedException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var task = await _tasks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Task {id} not found.");

        if (request.AssignedToId is { } assignee && assignee != task.AssignedToId
            && await _users.GetByIdAsync(assignee, ct) is null)
            throw new ValidationFailedException($"Assignee {assignee} does not exist.");

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        _tasks.Update(task);
        await _tasks.SaveChangesAsync(ct);

        var fresh = await _tasks.GetByIdWithCommentsAsync(task.Id, ct);
        return fresh!.ToDto();
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Task {id} not found.");

        // Ownership check: only the creator may delete.
        if (task.CreatedById != currentUserId)
            throw new ForbiddenException("Only the creator can delete this task.");

        _tasks.Remove(task);
        await _tasks.SaveChangesAsync(ct);
    }
}
