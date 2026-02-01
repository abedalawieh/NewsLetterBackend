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
        public IEnumerable<LookupDto> SubscriberTypes { get; set; }
        public IEnumerable<string> AvailableTemplates { get; set; }

        public class NewsletterInput
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string TargetSubscriberType { get; set; }
            public string TemplateName { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadLookupsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string[] TargetInterests)
        {
            if (!ModelState.IsValid) 
            {
                await LoadLookupsAsync();
                return Page();
            }

            var interests = TargetInterests.ToList();
            
            // Create newsletter with subscriber type targeting
            var newsletter = await _newsletterService.CreateDraftAsync(
                Input.Title, 
                Input.Content, 
                interests, 
                string.IsNullOrWhiteSpace(Input.TargetSubscriberType) ? null : Input.TargetSubscriberType
            );

            // Store template preference if specified
            if (!string.IsNullOrWhiteSpace(Input.TemplateName))
            {
                newsletter.TemplateName = Input.TemplateName;
            }

            TempData["SuccessMessage"] = "Newsletter draft created successfully!";
            return RedirectToPage("Index");
        }

        private async Task LoadLookupsAsync()
        {
            AvailableInterests = await _lookupService.GetItemsByCategoryAsync("Interest");
            SubscriberTypes = await _lookupService.GetItemsByCategoryAsync("SubscriberType");
            AvailableTemplates = _emailService.GetAvailableTemplates().ToList();
        }
    }
}
