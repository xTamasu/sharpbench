using TaskManager.Application.Dto.Comment;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using DomainTask = TaskManager.Domain.Entities.Task;

namespace TaskManager.Application.Services;

/// <summary>
/// Comment service implementation.
/// </summary>
public class CommentService : ICommentService
{
    private readonly IRepository<TaskComment> _commentRepository;
    private readonly IRepository<DomainTask> _taskRepository;
    private readonly IRepository<User> _userRepository;

    public CommentService(
        IRepository<TaskComment> commentRepository,
        IRepository<DomainTask> taskRepository,
        IRepository<User> userRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<CommentDto>> GetTaskCommentsAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task {taskId} not found.");

        var comments = await _commentRepository.GetAllAsync();
        return comments
            .Where(c => c.TaskId == taskId)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<CommentDto> CreateCommentAsync(Guid taskId, CreateCommentRequest request, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new KeyNotFoundException($"Task {taskId} not found.");

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            AuthorId = userId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();
        return MapToDto(comment);
    }

    public async Task<CommentDto> UpdateCommentAsync(Guid taskId, Guid commentId, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException($"Comment {commentId} not found.");

        if (comment.TaskId != taskId)
            throw new KeyNotFoundException($"Comment {commentId} not found on task {taskId}.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Only the comment author can edit it.");

        comment.Body = request.Body;
        comment.EditedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);
        await _commentRepository.SaveChangesAsync();
        return MapToDto(comment);
    }

    public async System.Threading.Tasks.Task DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException($"Comment {commentId} not found.");

        if (comment.TaskId != taskId)
            throw new KeyNotFoundException($"Comment {commentId} not found on task {taskId}.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Only the comment author can delete it.");

        await _commentRepository.DeleteAsync(comment);
        await _commentRepository.SaveChangesAsync();
    }

    private CommentDto MapToDto(TaskComment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorDisplayName = comment.Author?.DisplayName ?? "Unknown",
            Body = comment.Body,
            EditedAt = comment.EditedAt,
            CreatedAt = comment.CreatedAt
        };
    }
}
