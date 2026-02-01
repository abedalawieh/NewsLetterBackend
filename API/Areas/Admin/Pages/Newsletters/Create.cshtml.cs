using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Newsletters
{
    public class CreateModel : PageModel
    {
        private readonly INewsletterService _newsletterService;
        private readonly ILookupService _lookupService;
        private readonly IEmailService _emailService;

        public CreateModel(
            INewsletterService newsletterService, 
            ILookupService lookupService,
            IEmailService emailService)
        {
            _newsletterService = newsletterService;
            _lookupService = lookupService;
            _emailService = emailService;
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
            await LoadLookupsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string[] TargetInterests)
        {
            var errors = ModelState
    .Where(x => x.Value.Errors.Count > 0)
    .Select(x => new { Key = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() })
    .ToList();
            if (!ModelState.IsValid) 
            {
                await LoadLookupsAsync();
                return Page();
            }

            var interests = TargetInterests?.ToList() ?? new List<string>();
            
            // Create newsletter (no subscriber type targeting)
            var newsletter = await _newsletterService.CreateDraftAsync(
                Input.Title,
                Input.Content,
                interests,
                null
            );

            // Template is auto-selected per recipient; no explicit template stored here.

            TempData["SuccessMessage"] = "Newsletter draft created successfully!";
            return RedirectToPage("Index");
        }

        private async Task LoadLookupsAsync()
        {
            AvailableInterests = await _lookupService.GetItemsByCategoryAsync("Interest");
            // Subscriber types UI removed; no lookup required
            // Templates are auto-selected per recipient; no need to load template list here.
        }
    }
}
