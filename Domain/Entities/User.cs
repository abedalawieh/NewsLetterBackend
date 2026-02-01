using System;
using Microsoft.AspNetCore.Identity;

namespace NewsletterApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }


    public class ApplicationRole : IdentityRole<Guid>
    {
        public string Description { get; set; }
    }
}
