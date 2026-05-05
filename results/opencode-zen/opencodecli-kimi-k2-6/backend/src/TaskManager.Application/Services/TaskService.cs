using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

/// <summary>
/// Service for managing tasks, including CRUD operations and filtering.
/// </summary>
public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskFilterRequest? filter, CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Repository<TaskItem>().GetAllAsync(cancellationToken);

        // Apply filters if provided
        var query = tasks.AsEnumerable();

        if (filter?.Status.HasValue == true)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter?.Priority.HasValue == true)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (filter?.AssignedToId.HasValue == true)
        {
            query = query.Where(t => t.AssignedToId == filter.AssignedToId.Value);
        }

        return query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToResponse(t))
            .ToList();
    }

    public async Task<TaskDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(id, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID '{id}' was not found.");
        }

        return MapToDetailResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
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

        await _unitOfWork.Repository<TaskItem>().AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload to get navigation properties
        var createdTask = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(task.Id, cancellationToken);
        return MapToResponse(createdTask!);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(id, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID '{id}' was not found.");
        }

        // Update fields
        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<TaskItem>().UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedTask = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(task.Id, cancellationToken);
        return MapToResponse(updatedTask!);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(id, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID '{id}' was not found.");
        }

        // Only the task creator can delete the task
        if (task.CreatedById != userId)
        {
            throw new UnauthorizedAccessException("Only the task creator can delete this task.");
        }

        await _unitOfWork.Repository<TaskItem>().DeleteAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

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
            UpdatedAt = task.UpdatedAt
        };
    }

    private static TaskDetailResponse MapToDetailResponse(TaskItem task)
    {
        return new TaskDetailResponse
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
            Comments = task.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    TaskId = c.TaskId,
                    AuthorId = c.AuthorId,
                    AuthorName = c.Author.DisplayName,
                    Body = c.Body,
                    EditedAt = c.EditedAt,
                    CreatedAt = c.CreatedAt
                })
                .ToList()
        };
    }
}
