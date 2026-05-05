// Task service implementation handling CRUD operations with ownership checks.

using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskResponse>> GetTasksAsync(
        TaskStatus? status, Priority? priority, Guid? assignedToId)
    {
        var tasks = await _taskRepository.GetFilteredAsync(status, priority, assignedToId);
        return tasks.Select(MapToResponse);
    }

    public async Task<TaskDetailResponse> GetTaskByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetWithCommentsAsync(id)
            ?? throw new KeyNotFoundException($"Task with ID {id} not found.");

        return MapToDetailResponse(task);
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid createdById)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId,
            CreatedById = createdById,
            Status = TaskStatus.Todo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        // Reload with navigation properties for response mapping
        var created = await _taskRepository.GetWithCommentsAsync(task.Id);
        return MapToResponse(created!);
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId)
    {
        var task = await _taskRepository.GetWithCommentsAsync(id)
            ?? throw new KeyNotFoundException($"Task with ID {id} not found.");

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToResponse(task);
    }

    public async Task DeleteTaskAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Task with ID {id} not found.");

        // Only the creator can delete a task
        if (task.CreatedById != userId)
            throw new UnauthorizedAccessException("Only the task creator can delete this task.");

        await _taskRepository.DeleteAsync(task);
        await _taskRepository.SaveChangesAsync();
    }

    private static TaskResponse MapToResponse(TaskItem task)
    {
        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.AssignedTo != null
                ? new UserDto(task.AssignedTo.Id, task.AssignedTo.Email, task.AssignedTo.DisplayName)
                : null,
            new UserDto(task.CreatedBy.Id, task.CreatedBy.Email, task.CreatedBy.DisplayName),
            task.CreatedAt,
            task.UpdatedAt);
    }

    private static TaskDetailResponse MapToDetailResponse(TaskItem task)
    {
        return new TaskDetailResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.AssignedTo != null
                ? new UserDto(task.AssignedTo.Id, task.AssignedTo.Email, task.AssignedTo.DisplayName)
                : null,
            new UserDto(task.CreatedBy.Id, task.CreatedBy.Email, task.CreatedBy.DisplayName),
            task.CreatedAt,
            task.UpdatedAt,
            task.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponse(
                    c.Id,
                    c.TaskId,
                    new UserDto(c.Author.Id, c.Author.Email, c.Author.DisplayName),
                    c.Body,
                    c.EditedAt,
                    c.CreatedAt))
                .ToList());
    }
}
