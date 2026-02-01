using Microsoft.AspNetCore.Identity;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NewsletterApp.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserManager<ApplicationUser> userManager, ILogger<AdminService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAdminsAsync()
        {
            try
            {
                return _userManager.Users.OrderByDescending(u => u.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admins");
                throw;
            }
        }

        public async Task<IdentityResult> CreateAdminAsync(string username, string email, string firstName, string lastName, string password)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin");
                throw;
            }
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                return await _userManager.ResetPasswordAsync(user, token, newPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                throw;
            }
        }

        public async Task<IdentityResult> ToggleAdminStatusAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

                if (user.UserName.ToLower() == "admin") 
                    return IdentityResult.Failed(new IdentityError { Description = "Cannot deactivate master admin" });

                user.IsActive = !user.IsActive;
                return await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling admin status");
                throw;
            }
        }
    }
}
