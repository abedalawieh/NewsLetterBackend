using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.Application.DTOs
{
    public class CreateSubscriberDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Subscriber type is required")]
        public string Type { get; set; }

        [Required(ErrorMessage = "At least one communication method is required")]
        [MinLength(1, ErrorMessage = "At least one communication method must be selected")]
        public List<string> CommunicationMethods { get; set; }

        [Required(ErrorMessage = "At least one interest is required")]
        [MinLength(1, ErrorMessage = "At least one interest must be selected")]
        public List<string> Interests { get; set; }
    }

    public class UpdateSubscriberDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Subscriber type is required")]
        public string Type { get; set; }

        [Required(ErrorMessage = "At least one communication method is required")]
        [MinLength(1)]
        public List<string> CommunicationMethods { get; set; }

        [Required(ErrorMessage = "At least one interest is required")]
        [MinLength(1)]
        public List<string> Interests { get; set; }
    }

    public class SubscriberResponseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public List<string> CommunicationMethods { get; set; }
        public List<string> Interests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
    public class UnsubscribeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please tell us why you're unsubscribing.")]
        public string Reason { get; set; }
        [StringLength(500)]
        public string Comment { get; set; }
    }

    public class UnsubscribeHistoryDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>For admin analytics: reason and count of unsubscribes.</summary>
    public class UnsubscribeStatDto
    {
        public string Reason { get; set; }
        public int Count { get; set; }
    }
}

