// TaskService — business logic for task CRUD with ownership enforcement and filtering.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.DTOs.Comments;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TaskComment> _commentRepository;

    public TaskService(
        IRepository<TaskItem> taskRepository,
        IRepository<User> userRepository,
        IRepository<TaskComment> commentRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<TaskResponse>> GetAllAsync(TaskListFilter filter, CancellationToken ct = default)
    {
        var query = await _taskRepository.GetAllAsync(ct);

        // Apply filters
        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }
        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }
        if (filter.AssignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == filter.AssignedToId.Value);
        }

        var tasks = query
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        return tasks.Select(MapToResponse);
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id {id} not found.");
        }

        // Include comments for the detail view
        var commentsQuery = await _commentRepository.GetAllAsync(ct);
        var comments = commentsQuery
            .Where(c => c.TaskId == id)
            .OrderBy(c => c.CreatedAt)
            .ToList();

        task.Comments = comments;
        return MapToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(Guid createdById, CreateTaskRequest request, CancellationToken ct = default)
    {
        // Verify creator exists
        var creator = await _userRepository.GetByIdAsync(createdById, ct);
        if (creator == null)
        {
            throw new KeyNotFoundException($"User with id {createdById} not found.");
        }

        // Verify assigned user exists if provided
        if (request.AssignedToId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToId.Value, ct);
            if (assignedUser == null)
            {
                throw new KeyNotFoundException($"User with id {request.AssignedToId.Value} not found.");
            }
        }

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId,
            CreatedById = createdById,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _taskRepository.AddAsync(task, ct);

        // Set navigation properties for mapping
        task.CreatedBy = creator;
        if (request.AssignedToId.HasValue)
        {
            task.AssignedTo = await _userRepository.GetByIdAsync(request.AssignedToId.Value, ct);
        }

        return MapToResponse(task);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, Guid userId, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id {id} not found.");
        }

        // Verify assigned user exists if changed
        if (request.AssignedToId.HasValue && request.AssignedToId != task.AssignedToId)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToId.Value, ct);
            if (assignedUser == null)
            {
                throw new KeyNotFoundException($"User with id {request.AssignedToId.Value} not found.");
            }
            task.AssignedTo = assignedUser;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task, ct);

        return MapToResponse(task);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id {id} not found.");
        }

        // Only the task creator can delete
        if (task.CreatedById != userId)
        {
            throw new UnauthorizedAccessException("Only the task creator can delete a task.");
        }

        await _taskRepository.DeleteAsync(task, ct);
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
            CreatedByName = task.CreatedBy?.DisplayName,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Comments = task.Comments.Select(c => new CommentResponse
            {
                Id = c.Id,
                TaskId = c.TaskId,
                AuthorId = c.AuthorId,
                AuthorName = c.Author?.DisplayName ?? string.Empty,
                Body = c.Body,
                EditedAt = c.EditedAt,
                CreatedAt = c.CreatedAt
            })
        };
    }
}
