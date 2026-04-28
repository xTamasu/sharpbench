// Task service implementing all task business logic including ownership checks
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskResponse>> GetAllAsync(TaskItemStatus? status, TaskPriority? priority, Guid? assignedToId)
    {
        var tasks = await _taskRepository.GetFilteredAsync(status, priority, assignedToId);
        return tasks.Select(MapToResponse);
    }

    public async Task<TaskDetailResponse> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetWithCommentsAsync(id);
        if (task == null)
            throw new NotFoundException("Task not found.");

        return new TaskDetailResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.Priority.ToString(),
            task.DueDate,
            task.CreatedById,
            task.CreatedBy.DisplayName,
            task.AssignedToId,
            task.AssignedTo?.DisplayName,
            task.CreatedAt,
            task.UpdatedAt,
            task.Comments.Select(c => new CommentResponse(
                c.Id, c.TaskId, c.AuthorId, c.Author.DisplayName, c.Body, c.EditedAt, c.CreatedAt
            )).ToList()
        );
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId)
    {
        var task = new DomainTask
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            CreatedById = userId,
            AssignedToId = request.AssignedToId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        // Re-fetch to get navigation properties populated
        var created = await _taskRepository.GetWithCommentsAsync(task.Id);
        return MapToResponse(created!);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId)
    {
        var task = await _taskRepository.GetWithCommentsAsync(id);
        if (task == null)
            throw new NotFoundException("Task not found.");

        if (request.Title != null) task.Title = request.Title;
        if (request.Description != null) task.Description = request.Description;
        if (request.Status != null) task.Status = request.Status.Value;
        if (request.Priority != null) task.Priority = request.Priority.Value;
        if (request.DueDate != null) task.DueDate = request.DueDate;
        if (request.AssignedToId != null) task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        var updated = await _taskRepository.GetWithCommentsAsync(id);
        return MapToResponse(updated!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new NotFoundException("Task not found.");

        // Only the task creator can delete the task
        if (task.CreatedById != userId)
            throw new ForbiddenException("Only the task creator can delete this task.");

        await _taskRepository.DeleteAsync(task);
    }

    private static TaskResponse MapToResponse(DomainTask task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status.ToString(),
        task.Priority.ToString(),
        task.DueDate,
        task.CreatedById,
        task.CreatedBy?.DisplayName ?? string.Empty,
        task.AssignedToId,
        task.AssignedTo?.DisplayName,
        task.CreatedAt,
        task.UpdatedAt
    );
}