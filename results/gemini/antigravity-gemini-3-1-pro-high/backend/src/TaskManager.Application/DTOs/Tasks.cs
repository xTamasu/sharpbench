using System;
using System.Collections.Generic;
using TaskManager.Domain;

namespace TaskManager.Application.DTOs
{
    // Task Data Transfer Objects
    public record TaskResponse(Guid Id, string Title, string? Description, Status Status, Priority Priority, DateTime? DueDate, Guid? AssignedToId, Guid CreatedById, DateTime CreatedAt, DateTime UpdatedAt);
    
    public record TaskDetailResponse(Guid Id, string Title, string? Description, Status Status, Priority Priority, DateTime? DueDate, Guid? AssignedToId, Guid CreatedById, DateTime CreatedAt, DateTime UpdatedAt, IEnumerable<CommentResponse> Comments);

    public record CreateTaskRequest(string Title, string? Description, Priority Priority, DateTime? DueDate, Guid? AssignedToId);
    
    public record UpdateTaskRequest(string Title, string? Description, Status Status, Priority Priority, DateTime? DueDate, Guid? AssignedToId);

    // Comment DTOs
    public record CommentResponse(Guid Id, Guid TaskId, Guid AuthorId, string Body, DateTime? EditedAt, DateTime CreatedAt);
    
    public record CreateCommentRequest(string Body);
    
    public record UpdateCommentRequest(string Body);
}
