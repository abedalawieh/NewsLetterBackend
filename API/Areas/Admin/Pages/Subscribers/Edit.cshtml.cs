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

        [BindProperty]
        public UpdateSubscriberDto Input { get; set; }

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

            Input = new UpdateSubscriberDto
            {
                FirstName = Subscriber.FirstName,
                LastName = Subscriber.LastName,
                Type = Subscriber.Type,
                CommunicationMethods = Subscriber.CommunicationMethods,
                Interests = Subscriber.Interests
            };

            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, string[] SelectedMethods, string[] SelectedInterests)
        {
            Id = id;
            Input.CommunicationMethods = SelectedMethods?.ToList() ?? new List<string>();
            Input.Interests = SelectedInterests?.ToList() ?? new List<string>();

            if (!ModelState.IsValid)
            {
                Subscriber = await _subscriberService.GetSubscriberByIdAsync(id);
                await LoadLookupsAsync();
                return Page();
            }

            try
            {
                await _subscriberService.UpdateSubscriberAsync(id, Input);
                TempData["SuccessMessage"] = "Subscriber updated successfully.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                Subscriber = await _subscriberService.GetSubscriberByIdAsync(id);
                await LoadLookupsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostToggleAsync(Guid id)
        {
            try
            {
                var subscriber = await _subscriberService.GetSubscriberByIdAsync(id);
                if (subscriber == null) return NotFound();

                if (subscriber.IsActive)
                    await _subscriberService.DeactivateSubscriberAsync(id);
                else
                    await _subscriberService.ActivateSubscriberAsync(id);

                TempData["SuccessMessage"] = "Subscriber status updated successfully.";
                return RedirectToPage(new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { id });
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
