// Service interfaces — define contracts for all business logic.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.DTOs.Comments;

namespace TaskManager.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetAllAsync(TaskListFilter filter, CancellationToken ct = default);
    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskResponse> CreateAsync(Guid createdById, CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskResponse> UpdateAsync(Guid id, Guid userId, UpdateTaskRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}

public interface ICommentService
{
    Task<IEnumerable<CommentResponse>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task<CommentResponse> CreateAsync(Guid taskId, Guid authorId, CreateCommentRequest request, CancellationToken ct = default);
    Task<CommentResponse> UpdateAsync(Guid taskId, Guid commentId, Guid authorId, UpdateCommentRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid taskId, Guid commentId, Guid authorId, CancellationToken ct = default);
}
