using System;

namespace TaskManager.Domain
{
    // TaskComment Entity
    public class TaskComment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid TaskId { get; set; }
        public TaskEntity? Task { get; set; }

        public Guid AuthorId { get; set; }
        public User? Author { get; set; }

        public string Body { get; set; } = string.Empty;
        public DateTime? EditedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
