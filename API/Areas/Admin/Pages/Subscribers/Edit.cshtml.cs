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
    public class EditModel : PageModel
    {
        private readonly ISubscriberService _subscriberService;
        private readonly ILookupService _lookupService;

        public EditModel(ISubscriberService subscriberService, ILookupService lookupService)
        {
            _subscriberService = subscriberService;
            _lookupService = lookupService;
        }

        public SubscriberResponseDto Subscriber { get; set; }
        public Guid Id { get; set; }

        public IEnumerable<LookupDto> SubscriberTypes { get; set; }
        public IEnumerable<LookupDto> CommunicationMethods { get; set; }
        public IEnumerable<LookupDto> Interests { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Id = id;
            Subscriber = await _subscriberService.GetSubscriberByIdAsync(id);
            if (Subscriber == null) return NotFound();

            await LoadLookupsAsync();
            return Page();
        }

        private async Task LoadLookupsAsync()
        {
            SubscriberTypes = await _lookupService.GetItemsByCategoryAsync("SubscriberType");
            CommunicationMethods = await _lookupService.GetItemsByCategoryAsync("CommunicationMethod");
            Interests = await _lookupService.GetItemsByCategoryAsync("Interest");
        }
    }
}
