using System;
using System.Collections.Generic;
using NewsletterApp.Domain.Interfaces;

namespace NewsletterApp.Domain.Entities
{
    public class Newsletter : ISoftDelete, IAuditEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string TargetInterests { get; set; } // Comma separated or JSON
        public DateTime? SentAt { get; set; }
        public bool IsDraft { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public static Newsletter Create(string title, string content, string targetInterests)
        {
            return new Newsletter
            {
                Id = Guid.NewGuid(),
                Title = title,
                Content = content,
                TargetInterests = targetInterests,
                IsDraft = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public class AuditLog
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; } // Create, Update, Delete
        public string PerformedBy { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }

        public static AuditLog Create(string entityName, string entityId, string action, string performedBy, string details = "")
        {
            return new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow,
                Details = details
            };
        }
    }
}
