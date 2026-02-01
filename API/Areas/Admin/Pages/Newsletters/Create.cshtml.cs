using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Newsletters
{
    public class CreateModel : PageModel
    {
        private readonly INewsletterService _newsletterService;
        private readonly ILookupService _lookupService;

        public CreateModel(INewsletterService newsletterService, ILookupService lookupService)
        {
            _newsletterService = newsletterService;
            _lookupService = lookupService;
        }

        [BindProperty]
        public NewsletterInput Input { get; set; }

        public IEnumerable<LookupDto> AvailableInterests { get; set; }

        public class NewsletterInput
        {
            public string Title { get; set; }
            public string Content { get; set; }
        }

        public async Task OnGetAsync()
        {
            AvailableInterests = await _lookupService.GetItemsByCategoryAsync("Interest");
        }

        public async Task<IActionResult> OnPostAsync(string[] TargetInterests)
        {
            if (!ModelState.IsValid) 
            {
                AvailableInterests = await _lookupService.GetItemsByCategoryAsync("Interest");
                return Page();
            }

            var interests = TargetInterests.ToList();
            await _newsletterService.CreateDraftAsync(Input.Title, Input.Content, interests);

            return RedirectToPage("Index");
        }
    }
}

