using Microsoft.AspNetCore.Mvc;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Subscribers
{
    public class IndexModel : BasePaginatedPageModel
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

        public IEnumerable<SubscriberResponseDto> Subscribers { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public async Task OnGetAsync()
        {
            bool? isActive = null;
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                var normalized = StatusFilter.Trim().ToLowerInvariant();
                if (normalized == "active") isActive = true;
                if (normalized == "inactive") isActive = false;
            }

            var result = await _subscriberService.GetPagedSubscribersAsync(new SubscriberFilterParams
            {
                SearchTerm = SearchTerm,
                IsActive = isActive,
                PageNumber = PageNumber,
                PageSize = PageSize,
                SortBy = "CreatedAt",
                SortDescending = true
            });

            Subscribers = result.Items;
            Pagination = BuildPagination(result.TotalItems);
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

                SetSuccess("Subscriber status updated successfully");
            }
            catch
            {
                SetError("Error updating subscriber");
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
                SetSuccess("Subscriber deleted successfully");
            }
            catch
            {
                SetError("Error deleting subscriber");
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
