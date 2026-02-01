using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Infrastructure.Data;

namespace NewsletterApp.Infrastructure.Services
{
    public class DeletedItemsService : IDeletedItemsService
    {
        private readonly NewsletterDbContext _context;

        public DeletedItemsService(NewsletterDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<DeletedItemDto>> GetDeletedSubscribersAsync()
        {
            var list = await _context.Subscribers
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted)
                .OrderByDescending(s => s.DeletedAt)
                .Select(s => new DeletedItemDto
                {
                    Id = s.Id,
                    Type = "Subscriber",
                    Name = s.FirstName + " " + s.LastName,
                    Email = s.Email,
                    DeletedAt = s.DeletedAt ?? DateTime.UtcNow,
                    Details = "Type: " + s.Type + " | Interests: " + string.Join(", ", s.Interests.Take(2)),
                    SubscriberCount = 0
                })
                .ToListAsync();
            return list;
        }

        public async Task<IReadOnlyList<DeletedMetadataDto>> GetDeletedMetadataAsync()
        {
            var items = await _context.LookupItems
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

            var categories = await _context.LookupCategories
                .IgnoreQueryFilters()
                .Where(lc => lc.IsDeleted)
                .OrderByDescending(lc => lc.DeletedAt)
                .Select(lc => new DeletedMetadataDto
                {
                    Id = lc.Id,
                    Type = "LookupCategory",
                    Name = lc.Name,
                    Value = lc.Description ?? "",
                    Category = "Category",
                    DeletedAt = lc.DeletedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return items.Concat(categories).ToList();
        }

        public async Task<IReadOnlyList<DeletedNewsletterDto>> GetDeletedNewslettersAsync()
        {
            return await _context.Newsletters
                .IgnoreQueryFilters()
                .Where(n => n.IsDeleted)
                .OrderByDescending(n => n.DeletedAt)
                .Select(n => new DeletedNewsletterDto
                {
                    Id = n.Id,
                    Type = "Newsletter",
                    Title = n.Title,
                    Subject = n.TargetInterests ?? "",
                    DeletedAt = n.DeletedAt ?? DateTime.UtcNow,
                    CreatedAt = n.CreatedAt,
                    IsSent = n.SentAt.HasValue
                })
                .ToListAsync();
        }

        public async Task<ReplaceAndDeleteInfoDto> GetReplaceAndDeleteInfoAsync(Guid itemId, string category, string itemValue, string itemLabel, int subscriberCount)
        {
            var replacements = await _context.LookupItems
                .Where(li => li.Category.Name == category && !li.IsDeleted && li.Id != itemId)
                .Select(li => new { li.Value, li.Label })
                .ToListAsync();

            return new ReplaceAndDeleteInfoDto
            {
                ItemId = itemId,
                Category = category ?? "",
                OldValue = itemValue ?? "",
                ItemLabel = itemLabel ?? "",
                SubscriberCount = subscriberCount,
                Replacements = replacements.Select(r => new ReplacementOption { Value = r.Value, Label = r.Label }).ToList()
            };
        }

        public async Task<bool> RestoreAsync(Guid id, string type)
        {
            if (type == "Subscriber")
            {
                var entity = await _context.Subscribers.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);
                if (entity == null) return false;
                entity.IsDeleted = false;
                entity.DeletedAt = null;
            }
            else if (type == "LookupItem")
            {
                var entity = await _context.LookupItems.IgnoreQueryFilters().FirstOrDefaultAsync(li => li.Id == id);
                if (entity == null) return false;
                entity.IsDeleted = false;
                entity.DeletedAt = null;
            }
            else if (type == "LookupCategory")
            {
                var entity = await _context.LookupCategories.IgnoreQueryFilters().FirstOrDefaultAsync(lc => lc.Id == id);
                if (entity == null) return false;
                entity.IsDeleted = false;
                entity.DeletedAt = null;
            }
            else if (type == "Newsletter")
            {
                var entity = await _context.Newsletters.IgnoreQueryFilters().FirstOrDefaultAsync(n => n.Id == id);
                if (entity == null) return false;
                entity.IsDeleted = false;
                entity.DeletedAt = null;
            }
            else
                return false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> ReplaceAndDeleteAsync(Guid id, string category, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                return (false, "Please select a replacement option before proceeding.");

            var allSubscribers = await _context.Subscribers.Where(s => !s.IsDeleted).ToListAsync();
            var subscribers = allSubscribers.Where(s =>
                s.Type == oldValue ||
                (s.Interests != null && s.Interests.Contains(oldValue)) ||
                (s.CommunicationMethods != null && s.CommunicationMethods.Contains(oldValue))).ToList();

            int updatedCount = 0;
            foreach (var subscriber in subscribers)
            {
                bool modified = false;
                if (subscriber.Type == oldValue)
                {
                    var typeProp = typeof(Subscriber).GetProperty("Type");
                    typeProp?.SetValue(subscriber, newValue);
                    modified = true;
                }
                var interestIndex = subscriber.Interests?.IndexOf(oldValue) ?? -1;
                if (interestIndex >= 0)
                {
                    subscriber.Interests[interestIndex] = newValue;
                    modified = true;
                }
                var commIndex = subscriber.CommunicationMethods?.IndexOf(oldValue) ?? -1;
                if (commIndex >= 0)
                {
                    subscriber.CommunicationMethods[commIndex] = newValue;
                    modified = true;
                }
                if (modified)
                {
                    _context.Entry(subscriber).State = EntityState.Modified;
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();

            var item = await _context.LookupItems.IgnoreQueryFilters().FirstOrDefaultAsync(li => li.Id == id);
            if (item != null)
            {
                _context.LookupItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return (true, $"Successfully replaced '{oldValue}' with '{newValue}' for {updatedCount} subscriber(s) and deleted the item!");
        }

        public async Task<(bool Success, string ErrorMessage, ReplaceAndDeleteInfoDto ReplaceInfo)> PermanentlyDeleteAsync(Guid id, string type)
        {
            try
            {
                if (type == "LookupItem")
                {
                    var item = await _context.LookupItems.IgnoreQueryFilters().Include(li => li.Category).FirstOrDefaultAsync(li => li.Id == id);
                    if (item == null) return (false, "Item not found.", null);

                    var countByType = await _context.Subscribers.Where(s => !s.IsDeleted && s.Type == item.Value).CountAsync();
                    int countByLists = 0;
                    if (countByType == 0)
                    {
                        var activeSubscribers = await _context.Subscribers.Where(s => !s.IsDeleted).ToListAsync();
                        countByLists = activeSubscribers.Count(s =>
                            (s.Interests != null && s.Interests.Contains(item.Value)) ||
                            (s.CommunicationMethods != null && s.CommunicationMethods.Contains(item.Value)));
                    }
                    var total = countByType + countByLists;
                    if (total > 0)
                    {
                        var replaceInfo = await GetReplaceAndDeleteInfoAsync(id, item.Category?.Name ?? "", item.Value, item.Label, total);
                        return (false, $"Cannot delete '{item.Label}' - {total} subscriber(s) are using this option.", replaceInfo);
                    }
                    await _context.LookupItems.IgnoreQueryFilters().Where(li => li.Id == id).ExecuteDeleteAsync();
                }
                else if (type == "LookupCategory")
                {
                    var category = await _context.LookupCategories.IgnoreQueryFilters().FirstOrDefaultAsync(lc => lc.Id == id);
                    if (category == null) return (false, "Category not found.", null);
                    var itemsInCategory = await _context.LookupItems.IgnoreQueryFilters()
                        .CountAsync(li => li.CategoryId == category.Id && !li.IsDeleted);
                    if (itemsInCategory > 0)
                        return (false, $"Cannot delete '{category.Name}' - {itemsInCategory} active item(s) exist in this category.", null);
                    await _context.LookupCategories.IgnoreQueryFilters().Where(lc => lc.Id == id).ExecuteDeleteAsync();
                }
                else if (type == "Subscriber")
                {
                    await _context.Subscribers.IgnoreQueryFilters().Where(s => s.Id == id).ExecuteDeleteAsync();
                }
                else if (type == "Newsletter")
                {
                    await _context.Newsletters.IgnoreQueryFilters().Where(n => n.Id == id).ExecuteDeleteAsync();
                }
                else
                    return (false, "Invalid item type.", null);

                return (true, null, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }
}
