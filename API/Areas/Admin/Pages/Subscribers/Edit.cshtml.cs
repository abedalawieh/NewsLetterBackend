using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Subscribers
{
    public class EditModel : PageModel
    {
        private readonly ISubscriberService _subscriberService;

        public EditModel(ISubscriberService subscriberService)
        {
            _subscriberService = subscriberService;
        }

        public SubscriberResponseDto Subscriber { get; set; }
        public Guid Id { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Id = id;
            return await LoadAndShowAsync(id);
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

        private async Task<IActionResult> LoadAndShowAsync(Guid id)
        {
            try
            {
                Subscriber = await _subscriberService.GetSubscriberByIdAsync(id);
                if (Subscriber == null) return NotFound();
                Id = id;
                return Page();
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
