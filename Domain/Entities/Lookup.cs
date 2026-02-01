using System;
using System.Collections.Generic;
using NewsletterApp.Domain.Interfaces;

namespace NewsletterApp.Domain.Entities
{
    public class LookupCategory : ISoftDelete, IAuditEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // e.g., SubscriberType
        public string Description { get; set; }
        
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsSystem { get; set; } = false;

        public ICollection<LookupItem> Items { get; set; } = new List<LookupItem>();

        public static LookupCategory Create(string name, string description = "", bool isSystem = false)
        {
            return new LookupCategory
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = isSystem ? "System" : "Admin",
                IsSystem = isSystem
            };
        }
    }

    public class LookupItem : ISoftDelete, IAuditEntity
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public LookupCategory Category { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsSystem { get; set; } = false;

        public static LookupItem Create(Guid categoryId, string value, string label, int sortOrder = 0, bool isSystem = false)
        {
            return new LookupItem
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Value = value,
                Label = label,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = isSystem ? "System" : "Admin",
                IsSystem = isSystem
            };
        }
    }
}
