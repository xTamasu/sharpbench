// Domain entity representing a registered user
namespace TaskManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<DomainTask> CreatedTasks { get; set; } = new List<DomainTask>();
    public ICollection<DomainTask> AssignedTasks { get; set; } = new List<DomainTask>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}