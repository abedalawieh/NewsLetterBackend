using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.API.Areas.Admin.Pages.Admins
{
    public class ChangePasswordModel : PageModel
    {
        #region Dependencies

        private readonly UserManager<ApplicationUser> _userManager;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        #endregion

        #region Properties

        [BindProperty] public string UserId { get; set; }
        [BindProperty][Required][MinLength(6)] public string NewPassword { get; set; }
        public string TargetUsername { get; set; }

        #endregion

        #region Handlers

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            UserId = id;
            TargetUsername = user.UserName;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            #region Reset Password

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Security credentials for '{user.UserName}' have been successfully modified.";
                return RedirectToPage("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            #endregion

            TargetUsername = user.UserName;
            return Page();
        }

        #endregion
    }
}


