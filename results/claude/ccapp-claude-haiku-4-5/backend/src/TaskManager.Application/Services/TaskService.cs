using TaskManager.Application.Dto.Task;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using DomainTask = TaskManager.Domain.Entities.Task;
using DomainTaskStatus = TaskManager.Domain.Entities.TaskStatus;
using DomainTaskPriority = TaskManager.Domain.Entities.TaskPriority;

namespace TaskManager.Application.Services;

/// <summary>
/// Task service implementation.
/// </summary>
public class TaskService : ITaskService
{
    private readonly IRepository<DomainTask> _taskRepository;
    private readonly IRepository<User> _userRepository;

    public TaskService(IRepository<DomainTask> taskRepository, IRepository<User> userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(string? status = null, string? priority = null, string? assignedToId = null)
    {
        var tasks = await _taskRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(status) && int.TryParse(status, out var statusInt))
            tasks = tasks.Where(t => (int)t.Status == statusInt);

        if (!string.IsNullOrEmpty(priority) && int.TryParse(priority, out var priorityInt))
            tasks = tasks.Where(t => (int)t.Priority == priorityInt);

        if (!string.IsNullOrEmpty(assignedToId) && Guid.TryParse(assignedToId, out var assignedId))
            tasks = tasks.Where(t => t.AssignedToId == assignedId);

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException($"Task {id} not found.");

        return MapToDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid userId)
    {
        var task = new DomainTask
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = (DomainTaskPriority)request.Priority,
            Status = DomainTaskStatus.Todo,
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();
        return MapToDto(task);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException($"Task {id} not found.");

        if (task.CreatedById != userId)
            throw new UnauthorizedAccessException("Only the task creator can update it.");

        if (!string.IsNullOrEmpty(request.Title))
            task.Title = request.Title;

        if (request.Description != null)
            task.Description = request.Description;

        if (request.Status.HasValue)
            task.Status = (DomainTaskStatus)request.Status.Value;

        if (request.Priority.HasValue)
            task.Priority = (DomainTaskPriority)request.Priority.Value;

        if (request.DueDate != null)
            task.DueDate = request.DueDate;

        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.SaveChangesAsync();
        return MapToDto(task);
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException($"Task {id} not found.");

        if (task.CreatedById != userId)
            throw new UnauthorizedAccessException("Only the task creator can delete it.");

        await _taskRepository.DeleteAsync(task);
        await _taskRepository.SaveChangesAsync();
    }

    private TaskDto MapToDto(DomainTask task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = (int)task.Status,
            Priority = (int)task.Priority,
            DueDate = task.DueDate,
            AssignedToId = task.AssignedToId,
            AssignedToDisplayName = task.AssignedTo?.DisplayName,
            CreatedById = task.CreatedById,
            CreatedByDisplayName = task.CreatedBy?.DisplayName ?? "Unknown",
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
