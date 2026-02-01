using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Subscribers
{
    public class IndexModel : PageModel
    {
        private readonly ISubscriberService _subscriberService;

        public IndexModel(ISubscriberService subscriberService)
        {
            _subscriberService = subscriberService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public IEnumerable<SubscriberResponseDto> Subscribers { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public async Task OnGetAsync()
        {
            var allSubscribers = await _subscriberService.GetAllSubscribersAsync();
            var subscriberList = allSubscribers.ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                subscriberList = subscriberList
                    .Where(s => s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                s.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                s.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                subscriberList = StatusFilter.ToLower() == "active"
                    ? subscriberList.Where(s => s.IsActive).ToList()
                    : subscriberList.Where(s => !s.IsActive).ToList();
            }

            // Calculate pagination
            var totalItems = subscriberList.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            Subscribers = subscriberList.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();

            // Ensure valid page number
            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > totalPages && totalPages > 0) PageNumber = totalPages;

            Pagination = new PaginationViewModel
            {
                CurrentPage = PageNumber,
                TotalPages = Math.Max(1, totalPages),
                TotalItems = totalItems,
                PageSize = PageSize,
                PageParameterName = "pageNumber"
            };
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

                TempData["SuccessMessage"] = $"Subscriber status updated successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating subscriber: {ex.Message}";
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                statusFilter = StatusFilter,
                pageNumber = PageNumber,
                pageSize = PageSize
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                await _subscriberService.DeleteSubscriberAsync(id);
                TempData["SuccessMessage"] = "Subscriber deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting subscriber: {ex.Message}";
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                statusFilter = StatusFilter,
                pageNumber = PageNumber,
                pageSize = PageSize
            });
        }
    }
}
