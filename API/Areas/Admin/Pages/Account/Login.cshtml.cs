using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewsletterApp.API.Areas.Admin.Pages.Account
{
    public class LoginModel : PageModel
    {

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }



        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }



        public IActionResult OnGet(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/Dashboard");

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Admin/Dashboard");

            if (!ModelState.IsValid) return Page();


            var result = await _signInManager.PasswordSignInAsync(Username, Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(Username);
                
                if (user != null && !user.IsActive)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Access Denied: Your account has been deactivated by the system administrator.");
                    return Page();
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Contains("Admin"))
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Access Denied: You do not have Administrative privileges.");
                    return Page();
                }


                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                {
                    return RedirectToPage("/Dashboard", new { area = "Admin" });
                }

                return LocalRedirect(returnUrl);
            }


            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked due to too many failed attempts.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
            }


            return Page();
        }

    }
}
