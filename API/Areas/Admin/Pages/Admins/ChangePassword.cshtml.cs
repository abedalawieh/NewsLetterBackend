using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.API.Areas.Admin.Pages.Admins
{
    public class ChangePasswordModel : PageModel
    {

        private readonly UserManager<ApplicationUser> _userManager;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }



        [BindProperty] public string UserId { get; set; }
        [BindProperty][Required][MinLength(6)] public string NewPassword { get; set; }
        public string TargetUsername { get; set; }



        private static bool IsSystemAdmin(string username) =>
            string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase);

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (IsSystemAdmin(user.UserName))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Id != user.Id)
                {
                    TempData["ErrorMessage"] = "Only the system administrator can change their own password.";
                    return RedirectToPage("Index");
                }
            }

            UserId = id;
            TargetUsername = user.UserName;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            if (IsSystemAdmin(user.UserName))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Id != user.Id)
                {
                    TempData["ErrorMessage"] = "Only the system administrator can change their own password.";
                    return RedirectToPage("Index");
                }
            }


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Password for '{user.UserName}' has been successfully updated.";
                return RedirectToPage("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }


            TargetUsername = user.UserName;
            return Page();
        }

    }
}


