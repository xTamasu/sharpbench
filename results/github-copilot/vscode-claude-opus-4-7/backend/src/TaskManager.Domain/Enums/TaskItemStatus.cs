// TaskItemStatus.cs
// Lifecycle states for a TaskItem. Persisted as string in PostgreSQL for readability.
namespace TaskManager.Domain.Enums;

public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}
