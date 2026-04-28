using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Domain;

namespace TaskManager.Application.Interfaces
{
    // Business Service Interfaces
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }

    public interface ITaskService
    {
        Task<IEnumerable<TaskResponse>> GetTasksAsync(Status? status, Priority? priority, Guid? assignedToId);
        Task<TaskDetailResponse> GetTaskByIdAsync(Guid id);
        Task<TaskResponse> CreateTaskAsync(Guid userId, CreateTaskRequest request);
        Task<TaskResponse> UpdateTaskAsync(Guid id, Guid userId, UpdateTaskRequest request);
        Task DeleteTaskAsync(Guid id, Guid userId);
    }

    public interface ICommentService
    {
        Task<CommentResponse> AddCommentAsync(Guid taskId, Guid userId, CreateCommentRequest request);
        Task<CommentResponse> EditCommentAsync(Guid taskId, Guid commentId, Guid userId, UpdateCommentRequest request);
        Task DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId);
    }
}
