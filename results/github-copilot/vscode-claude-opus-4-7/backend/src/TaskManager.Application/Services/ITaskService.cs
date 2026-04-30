// ITaskService.cs
// Service contract for task CRUD and querying. The currentUserId is passed in
// explicitly so the service is independent of HTTP context.
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> ListAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? assignedToId,
        CancellationToken ct = default);

    Task<TaskDetailDto> GetAsync(Guid id, CancellationToken ct = default);

    Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid currentUserId, CancellationToken ct = default);

    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid currentUserId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
}
