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

        #region Properties

        public List<DeletedItemDto> DeletedItems { get; set; } = new();
        public List<DeletedMetadataDto> DeletedMetadata { get; set; } = new();
        public List<DeletedNewsletterDto> DeletedNewsletters { get; set; } = new();
        public PageHeaderViewModel PageHeader { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ItemType { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public PagedResult<DeletedItemDto> DeletedItemsPaged { get; set; } = new();
        public PagedResult<DeletedMetadataDto> DeletedMetadataPaged { get; set; } = new();
        public PagedResult<DeletedNewsletterDto> DeletedNewslettersPaged { get; set; } = new();

        public ReplaceAndDeleteInfoDto ReplaceAndDeleteInfo { get; set; }

        #endregion

        public async Task OnGetAsync()
        {
            PageHeader = new PageHeaderViewModel
            {
                Title = "Deleted Items Management",
                Subtitle = "View and manage soft-deleted items. You can restore or permanently delete them.",
                Icon = "fas fa-trash"
            };

            var subscribers = await _deletedItemsService.GetDeletedSubscribersAsync();
            var metadata = await _deletedItemsService.GetDeletedMetadataAsync();
            var newsletters = await _deletedItemsService.GetDeletedNewslettersAsync();

            DeletedItems = subscribers.OrderByDescending(x => x.DeletedAt).ToList();
            DeletedMetadata = metadata.OrderByDescending(x => x.DeletedAt).ToList();
            DeletedNewsletters = newsletters.OrderByDescending(x => x.DeletedAt).ToList();

            var itemIdStr = TempData["ItemId"]?.ToString();
            if (!string.IsNullOrEmpty(itemIdStr) && Guid.TryParse(itemIdStr, out var itemId))
            {
                var category = TempData["Category"]?.ToString();
                var itemValue = TempData["ItemValue"]?.ToString();
                var itemLabel = TempData["ItemLabel"]?.ToString();
                var subscriberCount = int.TryParse(TempData["SubscriberCount"]?.ToString(), out var c) ? c : 0;

                ReplaceAndDeleteInfo = await _deletedItemsService.GetReplaceAndDeleteInfoAsync(
                    itemId, category ?? "", itemValue ?? "", itemLabel ?? "", subscriberCount);
            }

            ApplyPagination();
        }

        private void ApplyPagination()
        {
            var skipCount = (PageNumber - 1) * PageSize;
            DeletedItemsPaged = new PagedResult<DeletedItemDto>
            {
                Items = DeletedItems.Skip(skipCount).Take(PageSize).ToList(),
                TotalItems = DeletedItems.Count,
                CurrentPage = PageNumber,
                PageSize = PageSize
            };
            DeletedMetadataPaged = new PagedResult<DeletedMetadataDto>
            {
                Items = DeletedMetadata.Skip(skipCount).Take(PageSize).ToList(),
                TotalItems = DeletedMetadata.Count,
                CurrentPage = PageNumber,
                PageSize = PageSize
            };
            DeletedNewslettersPaged = new PagedResult<DeletedNewsletterDto>
            {
                Items = DeletedNewsletters.Skip(skipCount).Take(PageSize).ToList(),
                TotalItems = DeletedNewsletters.Count,
                CurrentPage = PageNumber,
                PageSize = PageSize
            };
        }

        public async Task<IActionResult> OnPostRestoreAsync(Guid id, string type)
        {
            var ok = await _deletedItemsService.RestoreAsync(id, type);
            TempData[ok ? "Success" : "Error"] = ok ? "Item restored successfully!" : "Failed to restore item.";
            return RedirectToPage(new { ItemType = ItemType });
        }

        public async Task<IActionResult> OnPostReplaceAndDeleteAsync(Guid id, string category, string oldValue, string newValue)
        {
            var (success, message) = await _deletedItemsService.ReplaceAndDeleteAsync(id, category, oldValue, newValue ?? "");
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToPage(new { ItemType = ItemType });
        }

        public async Task<IActionResult> OnPostPermanentlyDeleteAsync(Guid id, string type)
        {
            var (success, errorMessage, replaceInfo) = await _deletedItemsService.PermanentlyDeleteAsync(id, type);

            if (success)
            {
                TempData["Success"] = "Item permanently deleted!";
                return RedirectToPage(new { ItemType = ItemType });
            }

            TempData["Error"] = errorMessage;
            if (replaceInfo != null)
            {
                TempData["ItemId"] = replaceInfo.ItemId.ToString();
                TempData["ItemValue"] = replaceInfo.OldValue;
                TempData["ItemLabel"] = replaceInfo.ItemLabel;
                TempData["Category"] = replaceInfo.Category;
                TempData["SubscriberCount"] = replaceInfo.SubscriberCount.ToString();
            }

            return RedirectToPage(new { ItemType = ItemType });
        }
    }
}
