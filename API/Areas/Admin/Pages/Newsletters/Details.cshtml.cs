using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Newsletters
{
    public class DetailsModel : PageModel
    {
        private readonly INewsletterService _newsletterService;

        public DetailsModel(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        public Newsletter Newsletter { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Newsletter = await _newsletterService.GetByIdAsync(id);
            
            if (Newsletter == null)
            {
                TempData["ErrorMessage"] = "Newsletter not found.";
                return RedirectToPage("Index");
            }

            return Page();
        }
    }
}
