using System;

namespace NewsletterApp.Application.DTOs
{
    public class NewsletterListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string TargetInterests { get; set; } = "";
        public string TemplateName { get; set; }
        public bool IsDraft { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NewsletterDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string TargetInterests { get; set; } = "";
        public string TemplateName { get; set; }
        public bool IsDraft { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
