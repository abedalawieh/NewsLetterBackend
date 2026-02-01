using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Deleted
{
    public class IndexModel : PageModel
    {
        private readonly IDeletedItemsService _deletedItemsService;

        public IndexModel(IDeletedItemsService deletedItemsService)
        {
            _deletedItemsService = deletedItemsService;
        }


        public List<DeletedNewsletterDto> DeletedNewsletters { get; set; } = new();
        public PageHeaderViewModel PageHeader { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public PagedResult<DeletedNewsletterDto> DeletedNewslettersPaged { get; set; } = new();


        public async Task OnGetAsync()
        {
            PageHeader = new PageHeaderViewModel
            {
                Title = "Deleted Newsletters",
                Subtitle = "View and manage soft-deleted newsletters. You can restore or permanently delete them.",
                Icon = "fas fa-trash"
            };

            var newsletters = await _deletedItemsService.GetDeletedNewslettersAsync();

            DeletedNewsletters = newsletters.OrderByDescending(x => x.DeletedAt).ToList();

            ApplyPagination();
        }

        private void ApplyPagination()
        {
            var skipCount = (PageNumber - 1) * PageSize;
            DeletedNewslettersPaged = new PagedResult<DeletedNewsletterDto>
            {
                Items = DeletedNewsletters.Skip(skipCount).Take(PageSize).ToList(),
                TotalItems = DeletedNewsletters.Count,
                CurrentPage = PageNumber,
                PageSize = PageSize
            };
        }

        public async Task<IActionResult> OnPostRestoreAsync(Guid id)
        {
            var ok = await _deletedItemsService.RestoreAsync(id, "Newsletter");
            TempData[ok ? "Success" : "Error"] = ok ? "Item restored successfully!" : "Failed to restore item.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostPermanentlyDeleteAsync(Guid id)
        {
            var (success, errorMessage, _) = await _deletedItemsService.PermanentlyDeleteAsync(id, "Newsletter");

            if (success)
            {
                TempData["Success"] = "Item permanently deleted!";
                return RedirectToPage();
            }

            TempData["Error"] = errorMessage;
            return RedirectToPage();
        }
    }
}
