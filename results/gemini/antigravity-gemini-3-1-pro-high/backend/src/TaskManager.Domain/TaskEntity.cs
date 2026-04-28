using System;
using System.Collections.Generic;

namespace TaskManager.Domain
{
    // Task Entity
    public class TaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Status Status { get; set; } = Status.Todo;
        public Priority Priority { get; set; } = Priority.Medium;
        public DateTime? DueDate { get; set; }
        
        public Guid? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }

        public Guid CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
}
