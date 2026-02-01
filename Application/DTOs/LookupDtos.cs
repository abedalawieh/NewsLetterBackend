using System;
using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.Application.DTOs
{
    public class LookupDto
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateLookupDto
    {
        [Required]
        public string Category { get; set; }
        [Required]
        public string Value { get; set; }
        [Required]
        public string Label { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateLookupDto
    {
        [Required]
        public string Label { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
