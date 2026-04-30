// User entity: represents a registered user of the system.
namespace TaskManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
