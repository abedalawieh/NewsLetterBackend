using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Deleted
{
    public class IndexModel : PageModel
    {
        private readonly NewsletterDbContext _context;

        public IndexModel(NewsletterDbContext context)
        {
            _context = context;
        }

        #region Properties

        public List<DeletedItemDto> DeletedItems { get; set; } = new();
        public List<DeletedMetadataDto> DeletedMetadata { get; set; } = new();
        public List<DeletedNewsletterDto> DeletedNewsletters { get; set; } = new();
        public List<LookupItem> AvailableReplacements { get; set; } = new();
        public PageHeaderViewModel PageHeader { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ItemType { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        // Pagination objects for each category
        public PagedResult<DeletedItemDto> DeletedItemsPaged { get; set; } = new();
        public PagedResult<DeletedMetadataDto> DeletedMetadataPaged { get; set; } = new();
        public PagedResult<DeletedNewsletterDto> DeletedNewslettersPaged { get; set; } = new();

        [BindProperty]
        public string? ReplacementValue { get; set; }

        public ReplaceAndDeleteInfoViewModel ReplaceAndDeleteInfo { get; set; }

        #endregion

        public async Task OnGetAsync()
        {
            PageHeader = new PageHeaderViewModel
            {
                Title = "Deleted Items Management",
                Subtitle = "View and manage soft-deleted items. You can restore or permanently delete them.",
                Icon = "fas fa-trash"
            };
            
            await LoadDeletedSubscribers();
            await LoadDeletedMetadata();
            await LoadDeletedNewsletters();
            
            // Check if we have Replace-and-Delete context from failed permanent delete
            var itemIdStr = TempData["ItemId"]?.ToString();
            if (!string.IsNullOrEmpty(itemIdStr) && Guid.TryParse(itemIdStr, out var itemId))
            {
                var category = TempData["Category"]?.ToString();
                var itemValue = TempData["ItemValue"]?.ToString();
                var itemLabel = TempData["ItemLabel"]?.ToString();
                var subscriberCount = TempData["SubscriberCount"]?.ToString() ?? "0";
                
                var replacements = await _context.LookupItems
                    .Where(li => li.Category.Name == category && !li.IsDeleted && li.Id != itemId)
                    .Select(li => new { li.Value, li.Label })
                    .ToListAsync();
                
                ReplaceAndDeleteInfo = new ReplaceAndDeleteInfoViewModel
                {
                    ItemId = itemId,
                    Category = category ?? "",
                    OldValue = itemValue ?? "",
                    ItemLabel = itemLabel ?? "",
                    SubscriberCount = int.TryParse(subscriberCount, out var c) ? c : 0,
                    Replacements = replacements.Select(r => (r.Value, r.Label)).ToList()
                };
            }
            
            // Apply pagination
            ApplyPagination();
        }

        private void ApplyPagination()
        {
            var skipCount = (PageNumber - 1) * PageSize;

            // Paginate subscribers
            DeletedItemsPaged = new PagedResult<DeletedItemDto>
            {
                Items = DeletedItems.Skip(skipCount).Take(PageSize).ToList(),
                TotalItems = DeletedItems.Count,
                CurrentPage = PageNumber,
                PageSize = PageSize
            };

            // Paginate metadata
            DeletedMetadataPaged = new PagedResult<DeletedMetadataDto>
            {
                Items = DeletedMetadata.Skip(skipCount).Take(PageSize).ToList(),
                TotalItems = DeletedMetadata.Count,
                CurrentPage = PageNumber,
                PageSize = PageSize
            };

            // Paginate newsletters
            DeletedNewslettersPaged = new PagedResult<DeletedNewsletterDto>
            {
                Items = DeletedNewsletters.Skip(skipCount).Take(PageSize).ToList(),
                TotalItems = DeletedNewsletters.Count,
                CurrentPage = PageNumber,
                PageSize = PageSize
            };
        }

        private async Task LoadDeletedSubscribers()
        {
            var deletedSubs = await _context.Subscribers
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted)
                .OrderByDescending(s => s.DeletedAt)
                .Select(s => new DeletedItemDto
                {
                    Id = s.Id,
                    Type = "Subscriber",
                    Name = $"{s.FirstName} {s.LastName}",
                    Email = s.Email,
                    DeletedAt = s.DeletedAt ?? DateTime.UtcNow,
                    Details = $"Type: {s.Type} | Interests: {string.Join(", ", s.Interests.Take(2))}",
                    SubscriberCount = 0 // Subscribers don't have subscriptions
                })
                .ToListAsync();

            DeletedItems.AddRange(deletedSubs);
        }

        private async Task LoadDeletedMetadata()
        {
            // Get deleted lookup items
            var deletedItems = await _context.LookupItems
                .IgnoreQueryFilters()
                .Include(li => li.Category)
                .Where(li => li.IsDeleted)
                .OrderByDescending(li => li.DeletedAt)
                .Select(li => new DeletedMetadataDto
                {
                    Id = li.Id,
                    Type = "LookupItem",
                    Name = li.Label,
                    Value = li.Value,
                    Category = li.Category.Name,
                    DeletedAt = li.DeletedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            DeletedMetadata.AddRange(deletedItems);

            // Get deleted lookup categories
            var deletedCategories = await _context.LookupCategories
                .IgnoreQueryFilters()
                .Where(lc => lc.IsDeleted)
                .OrderByDescending(lc => lc.DeletedAt)
                .Select(lc => new DeletedMetadataDto
                {
                    Id = lc.Id,
                    Type = "LookupCategory",
                    Name = lc.Name,
                    Value = lc.Description,
                    Category = "Category",
                    DeletedAt = lc.DeletedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            DeletedMetadata.AddRange(deletedCategories);
        }

        private async Task LoadDeletedNewsletters()
        {
            var deletedNewsletters = await _context.Newsletters
                .IgnoreQueryFilters()
                .Where(n => n.IsDeleted)
                .OrderByDescending(n => n.DeletedAt)
                .Select(n => new DeletedNewsletterDto
                {
                    Id = n.Id,
                    Type = "Newsletter",
                    Title = n.Title,
                    Subject = n.TargetInterests,
                    DeletedAt = n.DeletedAt ?? DateTime.UtcNow,
                    CreatedAt = n.CreatedAt,
                    IsSent = n.SentAt.HasValue
                })
                .ToListAsync();

            DeletedNewsletters.AddRange(deletedNewsletters);
        }

        public async Task<IActionResult> OnPostRestoreAsync(Guid id, string type)
        {
            try
            {
                if (type == "Subscriber")
                {
                    var subscriber = await _context.Subscribers.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);
                    if (subscriber != null)
                    {
                        subscriber.IsDeleted = false;
                        subscriber.DeletedAt = null;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "LookupItem")
                {
                    var item = await _context.LookupItems.IgnoreQueryFilters().FirstOrDefaultAsync(li => li.Id == id);
                    if (item != null)
                    {
                        item.IsDeleted = false;
                        item.DeletedAt = null;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "LookupCategory")
                {
                    var category = await _context.LookupCategories.IgnoreQueryFilters().FirstOrDefaultAsync(lc => lc.Id == id);
                    if (category != null)
                    {
                        category.IsDeleted = false;
                        category.DeletedAt = null;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type == "Newsletter")
                {
                    var newsletter = await _context.Newsletters.IgnoreQueryFilters().FirstOrDefaultAsync(n => n.Id == id);
                    if (newsletter != null)
                    {
                        newsletter.IsDeleted = false;
                        newsletter.DeletedAt = null;
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] = "Item restored successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error restoring item: {ex.Message}";
            }

            return RedirectToPage(new { ItemType = ItemType });
        }

        public async Task<IActionResult> OnGetCheckReplacementsAsync(Guid id, string category)
        {
            try
            {
                var item = await _context.LookupItems.IgnoreQueryFilters()
                    .Include(li => li.Category)
                    .FirstOrDefaultAsync(li => li.Id == id);

                if (item == null)
                    return NotFound();

                // Get all active lookup items in the same category except the current one
                var replacements = await _context.LookupItems
                    .Where(li => li.Category.Name == category && !li.IsDeleted && li.Id != id)
                    .ToListAsync();

                // Load all active subscribers into memory, then check
                var allSubscribers = await _context.Subscribers
                    .Where(s => !s.IsDeleted)
                    .ToListAsync();

                // Check in-memory which subscribers use this item
                var subscribersUsingItem = allSubscribers.Count(s => 
                    s.Type == item.Value || 
                    s.Interests.Contains(item.Value) || 
                    s.CommunicationMethods.Contains(item.Value));

                return new JsonResult(new
                {
                    itemId = id,
                    itemName = item.Label,
                    itemValue = item.Value,
                    category = category,
                    subscriberCount = subscribersUsingItem,
                    replacements = replacements.Select(r => new { id = r.Id, label = r.Label, value = r.Value }).ToList()
                });
            }
            catch (Exception)
            {
                return new JsonResult(new { error = "An error occurred while loading replacement options. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostReplaceAndDeleteAsync(Guid id, string category, string oldValue, string? newValue)
        {
            try
            {
                if (string.IsNullOrEmpty(newValue))
                {
                    TempData["Error"] = "Please select a replacement option before proceeding.";
                    return RedirectToPage(new { ItemType = ItemType });
                }

                // Load all subscribers into memory first
                var allSubscribers = await _context.Subscribers
                    .Where(s => !s.IsDeleted)
                    .ToListAsync();

                // Find subscribers using the old value (in-memory)
                var subscribers = allSubscribers.Where(s => 
                    s.Type == oldValue || 
                    s.Interests.Contains(oldValue) || 
                    s.CommunicationMethods.Contains(oldValue))
                    .ToList();

                int updatedCount = 0;

                // Replace the value for all matching subscribers
                foreach (var subscriber in subscribers)
                {
                    bool modified = false;

                    // Replace Type (uses reflection due to private setter)
                    if (subscriber.Type == oldValue)
                    {
                        var typeProp = typeof(Subscriber).GetProperty("Type");
                        typeProp?.SetValue(subscriber, newValue);
                        modified = true;
                    }

                    // Replace in Interests
                    var interestIndex = subscriber.Interests.IndexOf(oldValue);
                    if (interestIndex >= 0)
                    {
                        subscriber.Interests[interestIndex] = newValue;
                        modified = true;
                    }

                    // Replace in CommunicationMethods
                    var commIndex = subscriber.CommunicationMethods.IndexOf(oldValue);
                    if (commIndex >= 0)
                    {
                        subscriber.CommunicationMethods[commIndex] = newValue;
                        modified = true;
                    }

                    if (modified)
                    {
                        _context.Entry(subscriber).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        updatedCount++;
                    }
                }

                // Delete the lookup item
                var item = await _context.LookupItems.IgnoreQueryFilters().FirstOrDefaultAsync(li => li.Id == id);
                if (item != null)
                {
                    _context.LookupItems.Remove(item);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"Successfully replaced '{oldValue}' with '{newValue}' for {updatedCount} subscriber(s) and deleted the item!";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while replacing and deleting the item. Please try again or contact support if the problem persists.";
            }

            return RedirectToPage(new { ItemType = ItemType });
        }

        public async Task<IActionResult> OnPostPermanentlyDeleteAsync(Guid id, string type)
        {
            try
            {
                if (type == "LookupItem")
                {
                    // Load item first (needed for error messages + checks)
                    var item = await _context.LookupItems
                        .IgnoreQueryFilters()
                        .Include(li => li.Category)
                        .FirstOrDefaultAsync(li => li.Id == id);

                    if (item == null)
                    {
                        TempData["Error"] = "Item not found.";
                        return RedirectToPage(new { ItemType = ItemType });
                    }

                    // 1) DB check for Type usage (translatable)
                    var countByType = await _context.Subscribers
                        .Where(s => !s.IsDeleted && s.Type == item.Value)
                        .CountAsync();

                    // 2) In-memory check for List<string> fields (NOT translatable)
                    int countByLists = 0;
                    if (countByType == 0) // optional: skip loading if already used by Type
                    {
                        var activeSubscribers = await _context.Subscribers
                            .Where(s => !s.IsDeleted)
                            .ToListAsync();

                        countByLists = activeSubscribers.Count(s =>
                            (s.Interests != null && s.Interests.Contains(item.Value)) ||
                            (s.CommunicationMethods != null && s.CommunicationMethods.Contains(item.Value))
                        );
                    }

                    var subscribersUsingItem = countByType + countByLists;

                    if (subscribersUsingItem > 0)
                    {
                        TempData["Error"] = $"Cannot delete '{item.Label}' - {subscribersUsingItem} subscriber(s) are using this option.";
                        TempData["ItemId"] = id.ToString();
                        TempData["ItemValue"] = item.Value;
                        TempData["ItemLabel"] = item.Label;
                        TempData["Category"] = item.Category?.Name ?? "";
                        TempData["SubscriberCount"] = subscribersUsingItem.ToString();

                        return RedirectToPage(new { ItemType = ItemType });
                    }

                    // Hard delete from DB (bypasses your soft delete behavior)
                    await _context.LookupItems
                        .IgnoreQueryFilters()
                        .Where(li => li.Id == id)
                        .ExecuteDeleteAsync();
                }
                else if (type == "LookupCategory")
                {
                    var category = await _context.LookupCategories
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(lc => lc.Id == id);

                    if (category == null)
                    {
                        TempData["Error"] = "Category not found.";
                        return RedirectToPage(new { ItemType = ItemType });
                    }

                    // block delete if active items exist in category
                    var itemsInCategory = await _context.LookupItems
                        .IgnoreQueryFilters()
                        .Where(li => li.Category.Id == category.Id && !li.IsDeleted)
                        .CountAsync();

                    if (itemsInCategory > 0)
                    {
                        TempData["Error"] = $"Cannot delete '{category.Name}' - {itemsInCategory} active item(s) exist in this category. Delete or archive all items first.";
                        return RedirectToPage(new { ItemType = ItemType });
                    }

                    await _context.LookupCategories
                        .IgnoreQueryFilters()
                        .Where(lc => lc.Id == id)
                        .ExecuteDeleteAsync();
                }
                else if (type == "Subscriber")
                {
                    await _context.Subscribers
                        .IgnoreQueryFilters()
                        .Where(s => s.Id == id)
                        .ExecuteDeleteAsync();
                }
                else if (type == "Newsletter")
                {
                    await _context.Newsletters
                        .IgnoreQueryFilters()
                        .Where(n => n.Id == id)
                        .ExecuteDeleteAsync();
                }
                else
                {
                    TempData["Error"] = "Invalid item type.";
                    return RedirectToPage(new { ItemType = ItemType });
                }

                TempData["Success"] = "Item permanently deleted!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting item: {ex.Message}";
            }

            return RedirectToPage(new { ItemType = ItemType });
        }

    }

    public class DeletedItemDto
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime DeletedAt { get; set; }
        public string? Details { get; set; }
        public int SubscriberCount { get; set; }
    }

    public class DeletedMetadataDto
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? Category { get; set; }
        public DateTime DeletedAt { get; set; }
    }

    public class DeletedNewsletterDto
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Subject { get; set; }
        public DateTime DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSent { get; set; }
    }

    public class ReplaceAndDeleteInfoViewModel
    {
        public Guid ItemId { get; set; }
        public string Category { get; set; } = "";
        public string OldValue { get; set; } = "";
        public string ItemLabel { get; set; } = "";
        public int SubscriberCount { get; set; }
        public List<(string Value, string Label)> Replacements { get; set; } = new();
    }
}
