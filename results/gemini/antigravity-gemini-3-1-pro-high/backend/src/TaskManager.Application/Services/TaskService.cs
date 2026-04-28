using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Services
{
    public interface IExtendedTaskRepository : IRepository<TaskEntity>
    {
        Task<IEnumerable<TaskEntity>> GetTasksWithDetailsAsync(Status? status, Priority? priority, Guid? assignedToId);
        Task<TaskEntity?> GetTaskWithCommentsAsync(Guid id);
    }

    public class TaskService : ITaskService
    {
        private readonly IExtendedTaskRepository _taskRepository;
        private readonly IRepository<User> _userRepository;

        public TaskService(IExtendedTaskRepository taskRepository, IRepository<User> userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<TaskResponse>> GetTasksAsync(Status? status, Priority? priority, Guid? assignedToId)
        {
            var tasks = await _taskRepository.GetTasksWithDetailsAsync(status, priority, assignedToId);
            return tasks.Select(t => new TaskResponse(t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate, t.AssignedToId, t.CreatedById, t.CreatedAt, t.UpdatedAt));
        }

        public async Task<TaskDetailResponse> GetTaskByIdAsync(Guid id)
        {
            var task = await _taskRepository.GetTaskWithCommentsAsync(id);
            if (task == null) throw new KeyNotFoundException("Task not found");

            var comments = task.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentResponse(c.Id, c.TaskId, c.AuthorId, c.Body, c.EditedAt, c.CreatedAt));
            return new TaskDetailResponse(task.Id, task.Title, task.Description, task.Status, task.Priority, task.DueDate, task.AssignedToId, task.CreatedById, task.CreatedAt, task.UpdatedAt, comments);
        }

        public async Task<TaskResponse> CreateTaskAsync(Guid userId, CreateTaskRequest request)
        {
            var task = new TaskEntity
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                DueDate = request.DueDate,
                AssignedToId = request.AssignedToId,
                CreatedById = userId,
                Status = Status.Todo
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            return new TaskResponse(task.Id, task.Title, task.Description, task.Status, task.Priority, task.DueDate, task.AssignedToId, task.CreatedById, task.CreatedAt, task.UpdatedAt);
        }

        public async Task<TaskResponse> UpdateTaskAsync(Guid id, Guid userId, UpdateTaskRequest request)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) throw new KeyNotFoundException("Task not found");

            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.AssignedToId = request.AssignedToId;
            task.UpdatedAt = DateTime.UtcNow;

            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync();

            return new TaskResponse(task.Id, task.Title, task.Description, task.Status, task.Priority, task.DueDate, task.AssignedToId, task.CreatedById, task.CreatedAt, task.UpdatedAt);
        }

        public async Task DeleteTaskAsync(Guid id, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) throw new KeyNotFoundException("Task not found");

            if (task.CreatedById != userId) throw new UnauthorizedAccessException("Only the task creator can delete the task");

            _taskRepository.Remove(task);
            await _taskRepository.SaveChangesAsync();
        }
    }
}
