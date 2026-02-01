using System;
using System.Collections.Generic;

namespace NewsletterApp.Application.DTOs
{
    public class DeletedItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DeletedAt { get; set; }
        public string Details { get; set; }
        public int SubscriberCount { get; set; }
    }

    public class DeletedMetadataDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public string Name { get; set; }
        public string Value { get; set; }
        public string Category { get; set; }
        public DateTime DeletedAt { get; set; }
    }

    public class DeletedNewsletterDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public string Title { get; set; }
        public string Subject { get; set; }
        public DateTime DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSent { get; set; }
    }

    public class ReplaceAndDeleteInfoDto
    {
        public Guid ItemId { get; set; }
        public string Category { get; set; } = "";
        public string OldValue { get; set; } = "";
        public string ItemLabel { get; set; } = "";
        public int SubscriberCount { get; set; }
        public List<(string Value, string Label)> Replacements { get; set; } = new();
    }
}
