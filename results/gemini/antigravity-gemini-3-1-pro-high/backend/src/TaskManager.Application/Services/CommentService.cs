using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain;

namespace TaskManager.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<TaskComment> _commentRepository;
        private readonly IExtendedTaskRepository _taskRepository;

        public CommentService(IRepository<TaskComment> commentRepository, IExtendedTaskRepository taskRepository)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
        }

        public async Task<CommentResponse> AddCommentAsync(Guid taskId, Guid userId, CreateCommentRequest request)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) throw new KeyNotFoundException("Task not found");

            var comment = new TaskComment
            {
                TaskId = taskId,
                AuthorId = userId,
                Body = request.Body
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            return new CommentResponse(comment.Id, comment.TaskId, comment.AuthorId, comment.Body, comment.EditedAt, comment.CreatedAt);
        }

        public async Task<CommentResponse> EditCommentAsync(Guid taskId, Guid commentId, Guid userId, UpdateCommentRequest request)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.TaskId != taskId) throw new KeyNotFoundException("Comment not found");

            if (comment.AuthorId != userId) throw new UnauthorizedAccessException("Only the comment author can edit it");

            comment.Body = request.Body;
            comment.EditedAt = DateTime.UtcNow;

            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();

            return new CommentResponse(comment.Id, comment.TaskId, comment.AuthorId, comment.Body, comment.EditedAt, comment.CreatedAt);
        }

        public async Task DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.TaskId != taskId) throw new KeyNotFoundException("Comment not found");

            if (comment.AuthorId != userId) throw new UnauthorizedAccessException("Only the comment author can delete it");

            _commentRepository.Remove(comment);
            await _commentRepository.SaveChangesAsync();
        }
    }
}
