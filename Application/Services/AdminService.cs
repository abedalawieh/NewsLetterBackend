using Microsoft.AspNetCore.Identity;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAdminsAsync()
        {
            return _userManager.Users.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public async Task<IdentityResult> CreateAdminAsync(string username, string email, string firstName, string lastName, string password)
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> ToggleAdminStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            if (user.UserName.ToLower() == "admin") 
                return IdentityResult.Failed(new IdentityError { Description = "Cannot deactivate master admin" });

            user.IsActive = !user.IsActive;
            return await _userManager.UpdateAsync(user);
        }
    }
}
