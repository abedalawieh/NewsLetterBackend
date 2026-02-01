using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Admins
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<ApplicationUser> Administrators { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public PaginationViewModel Pagination { get; set; }

        public void OnGet()
        {
            var allAdmins = _userManager.Users.OrderByDescending(u => u.CreatedAt).ToList();
            
            var totalItems = allAdmins.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            Administrators = allAdmins.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();

            Pagination = new PaginationViewModel
            {
                CurrentPage = PageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = PageSize,
                PageParameterName = "pageNumber"
            };
        }

        public bool IsSystemAdmin(string username)
        {
            return string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsCurrentUserSystemAdmin()
        {
            var name = User.Identity?.Name;
            return IsSystemAdmin(name ?? "");
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                if (IsSystemAdmin(user.UserName))
                {
                    TempData["ErrorMessage"] = "Cannot modify the system administrator account.";
                    return RedirectToPage();
                }

                user.IsActive = !user.IsActive;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Administrator status updated successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update administrator status";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToPage(new { pageNumber = PageNumber });
        }
    }
}
