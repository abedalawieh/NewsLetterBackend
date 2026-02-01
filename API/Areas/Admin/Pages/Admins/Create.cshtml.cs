using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.API.Areas.Admin.Pages.Admins
{
    public class CreateModel : PageModel
    {
        #region Dependencies

        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        #endregion

        #region Properties

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required] public string Username { get; set; }
            [Required][EmailAddress] public string Email { get; set; }
            [Required] public string FirstName { get; set; }
            [Required] public string LastName { get; set; }
            [Required][MinLength(6)] public string Password { get; set; }
        }

        #endregion

        #region Handlers

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            #region Create User

            var user = new ApplicationUser
            {
                UserName = Input.Username,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["Message"] = $"Administrative profile '{Input.Username}' has been successfully deployed.";
                return RedirectToPage("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            #endregion

            return Page();
        }

        #endregion
    }
}

