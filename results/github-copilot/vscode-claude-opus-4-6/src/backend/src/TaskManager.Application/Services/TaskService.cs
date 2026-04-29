// Task service: handles all task CRUD operations and business rules.
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<TaskResponse>> GetAllAsync(TaskItemStatus? status, TaskPriority? priority, Guid? assignedToId)
    {
        var tasks = await _taskRepository.GetFilteredAsync(status, priority, assignedToId);
        return tasks.Select(MapToResponse);
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdWithCommentsAsync(id);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id '{id}' not found");
        }

        return MapToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        // Reload with navigation properties
        var created = await _taskRepository.GetByIdWithCommentsAsync(task.Id);
        return MapToResponse(created!);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id '{id}' not found");
        }

        // Update fields and set UpdatedAt timestamp
        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        var updated = await _taskRepository.GetByIdWithCommentsAsync(task.Id);
        return MapToResponse(updated!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id '{id}' not found");
        }

        // Only the task creator can delete — ownership check
        if (task.CreatedById != userId)
        {
            throw new UnauthorizedAccessException("Only the task creator can delete this task");
        }

        _taskRepository.Remove(task);
        await _taskRepository.SaveChangesAsync();
    }

    // Maps a TaskItem entity to its response DTO.
    private static TaskResponse MapToResponse(TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo?.DisplayName,
            CreatedById = task.CreatedById,
            CreatedByName = task.CreatedBy?.DisplayName ?? string.Empty,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Comments = task.Comments?.Select(c => new CommentResponse
            {
                Id = c.Id,
                TaskId = c.TaskId,
                AuthorId = c.AuthorId,
                AuthorName = c.Author?.DisplayName ?? string.Empty,
                Body = c.Body,
                EditedAt = c.EditedAt,
                CreatedAt = c.CreatedAt
            }).ToList() ?? new List<CommentResponse>()
        };
    }
}
