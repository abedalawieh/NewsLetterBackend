using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Subscribers
{
    public class CreateModel : PageModel
    {
        private readonly ISubscriberService _subscriberService;
        private readonly ILookupService _lookupService;

        public CreateModel(ISubscriberService subscriberService, ILookupService lookupService)
        {
            _subscriberService = subscriberService;
            _lookupService = lookupService;
        }

        [BindProperty]
        public CreateSubscriberDto Input { get; set; }

        public IEnumerable<LookupDto> SubscriberTypes { get; set; }
        public IEnumerable<LookupDto> CommunicationMethods { get; set; }
        public IEnumerable<LookupDto> Interests { get; set; }

        public async Task OnGetAsync()
        {
            await LoadLookupsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string[] SelectedMethods, string[] SelectedInterests)
        {
            Input.CommunicationMethods = SelectedMethods?.ToList() ?? new List<string>();
            Input.Interests = SelectedInterests?.ToList() ?? new List<string>();

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return Page();
            }

            try
            {
                await _subscriberService.CreateSubscriberAsync(Input);
                TempData["SuccessMessage"] = "Subscriber created successfully.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadLookupsAsync();
                return Page();
            }
        }

        private async Task LoadLookupsAsync()
        {
            SubscriberTypes = await _lookupService.GetItemsByCategoryAsync("SubscriberType");
            CommunicationMethods = await _lookupService.GetItemsByCategoryAsync("CommunicationMethod");
            Interests = await _lookupService.GetItemsByCategoryAsync("Interest");
        }
    }
}
