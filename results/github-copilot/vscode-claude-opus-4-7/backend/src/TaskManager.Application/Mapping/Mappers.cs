// Mappers.cs
// Hand-rolled entity-to-DTO projections. Kept simple and explicit
// to avoid a dependency on AutoMapper.
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Mapping;

public static class Mappers
{
    public static UserDto ToDto(this User u) =>
        new(u.Id, u.Email, u.DisplayName);

    public static CommentDto ToDto(this TaskComment c) =>
        new(c.Id, c.TaskId, c.Author!.ToDto(), c.Body, c.CreatedAt, c.EditedAt);

    public static TaskDto ToDto(this TaskItem t) =>
        new(
            t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
            t.AssignedTo?.ToDto(),
            t.CreatedBy!.ToDto(),
            t.CreatedAt, t.UpdatedAt);

    public static TaskDetailDto ToDetailDto(this TaskItem t) =>
        new(
            t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
            t.AssignedTo?.ToDto(),
            t.CreatedBy!.ToDto(),
            t.CreatedAt, t.UpdatedAt,
            t.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => c.ToDto())
                .ToList());
}
