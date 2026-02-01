using Microsoft.AspNetCore.Identity;
using NewsletterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<ApplicationUser>> GetAllAdminsAsync();
        Task<IdentityResult> CreateAdminAsync(string username, string email, string firstName, string lastName, string password);
        Task<IdentityResult> ChangePasswordAsync(string userId, string newPassword);
        Task<IdentityResult> ToggleAdminStatusAsync(string userId);
    }
}
