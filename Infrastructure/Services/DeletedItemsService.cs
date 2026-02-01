using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace NewsletterApp.Infrastructure.Services
{
    public class DeletedItemsService : IDeletedItemsService
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly INewsletterRepository _newsletterRepository;
        private readonly ILogger<DeletedItemsService> _logger;

        public DeletedItemsService(
            ISubscriberRepository subscriberRepository,
            ILookupRepository lookupRepository,
            INewsletterRepository newsletterRepository,
            ILogger<DeletedItemsService> logger)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _newsletterRepository = newsletterRepository ?? throw new ArgumentNullException(nameof(newsletterRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<DeletedItemDto>> GetDeletedSubscribersAsync()
        {
            try
            {
                return await _subscriberRepository.AllEntities
                    .Where(s => s.IsDeleted)
                    .OrderByDescending(s => s.DeletedAt)
                    .Select(s => new DeletedItemDto
                    {
                        Id = s.Id,
                        Type = "Subscriber",
                        Name = (s.FirstName ?? "") + " " + (s.LastName ?? ""),
                        Email = s.Email,
                        DeletedAt = s.DeletedAt ?? DateTime.UtcNow,
                        Details = "Type: " + s.Type + " | Interests: " + string.Join(", ", s.Interests.Select(i => i.LookupItem.Value).Take(2)),
                        SubscriberCount = 0
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted subscribers");
                throw;
            }
        }

        public async Task<IReadOnlyList<DeletedMetadataDto>> GetDeletedMetadataAsync()
        {
            try
            {
                var items = await _lookupRepository.AllEntities
                    .Include(li => li.Category)
                    .Where(li => li.IsDeleted)
                    .OrderByDescending(li => li.DeletedAt)
                    .Select(li => new DeletedMetadataDto
                    {
                        Id = li.Id,
                        Type = "LookupItem",
                        Name = li.Label,
                        Value = li.Value,
                        Category = li.Category != null ? li.Category.Name : "None",
                        DeletedAt = li.DeletedAt ?? DateTime.UtcNow
                    })
                    .ToListAsync();

                var categories = await _lookupRepository.AllCategories
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted metadata");
                throw;
            }
        }

        public async Task<IReadOnlyList<DeletedNewsletterDto>> GetDeletedNewslettersAsync()
        {
            try
            {
                return await _newsletterRepository.AllEntities
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted newsletters");
                throw;
            }
        }

        public async Task<ReplaceAndDeleteInfoDto> GetReplaceAndDeleteInfoAsync(Guid itemId, string category, string itemValue, string itemLabel, int subscriberCount)
        {
            try
            {
                var replacements = await _lookupRepository.Entities
                    .Where(li => li.Category.Name == category && li.Id != itemId)
                    .Select(li => new ReplacementOption { Value = li.Value, Label = li.Label })
                    .ToListAsync();

                return new ReplaceAndDeleteInfoDto
                {
                    ItemId = itemId,
                    Category = category ?? "",
                    OldValue = itemValue ?? "",
                    ItemLabel = itemLabel ?? "",
                    SubscriberCount = subscriberCount,
                    Replacements = replacements
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building replace/delete info");
                throw;
            }
        }

        public async Task<bool> RestoreAsync(Guid id, string type)
        {
            try
            {
                switch (type)
                {
                    case "Subscriber":
                        var subscriber = await _subscriberRepository.AllEntities.FirstOrDefaultAsync(s => s.Id == id);
                        if (subscriber == null) return false;
                        subscriber.IsDeleted = false;
                        subscriber.DeletedAt = null;
                        await _subscriberRepository.UpdateAsync(subscriber);
                        break;
                    case "LookupItem":
                        var item = await _lookupRepository.AllEntities.FirstOrDefaultAsync(li => li.Id == id);
                        if (item == null) return false;
                        item.IsDeleted = false;
                        item.DeletedAt = null;
                        await _lookupRepository.UpdateAsync(item);
                        break;
                    case "LookupCategory":
                        var category = await _lookupRepository.AllCategories.FirstOrDefaultAsync(lc => lc.Id == id);
                        if (category == null) return false;
                        category.IsDeleted = false;
                        category.DeletedAt = null;
                        await _lookupRepository.UpdateCategoryAsync(category);
                        break;
                    case "Newsletter":
                        var newsletter = await _newsletterRepository.AllEntities.FirstOrDefaultAsync(n => n.Id == id);
                        if (newsletter == null) return false;
                        newsletter.IsDeleted = false;
                        newsletter.DeletedAt = null;
                        await _newsletterRepository.UpdateAsync(newsletter);
                        break;
                    default:
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring item {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> ReplaceAndDeleteAsync(Guid id, string category, string oldValue, string newValue)
        {
            try
            {
                if (string.IsNullOrEmpty(newValue))
                    return (false, "Please select a replacement option before proceeding.");

                var subscribers = await _subscriberRepository.Entities
                    .Include(s => s.Interests)
                    .ThenInclude(i => i.LookupItem)
                    .Include(s => s.CommunicationMethods)
                    .ThenInclude(c => c.LookupItem)
                    .ToListAsync();

                var targetSubscribers = subscribers.Where(s =>
                    s.Type == oldValue ||
                    s.Interests.Any(i => i.LookupItem.Value == oldValue) ||
                    s.CommunicationMethods.Any(c => c.LookupItem.Value == oldValue)).ToList();

                var newLookupItem = await _lookupRepository.Entities.FirstOrDefaultAsync(li => li.Value == newValue);
                int updatedCount = 0;
                foreach (var subscriber in targetSubscribers)
                {
                    bool modified = false;
                    if (subscriber.Type == oldValue)
                    {
                        subscriber.UpdateType(newValue);
                        modified = true;
                    }
                    
                    var interestItems = subscriber.Interests.Where(i => i.LookupItem.Value == oldValue).ToList();
                    foreach (var interestItem in interestItems)
                    {
                        if (newLookupItem != null)
                        {
                            interestItem.LookupItem = newLookupItem;
                            interestItem.LookupItemId = newLookupItem.Id;
                            modified = true;
                        }
                    }
                    
                    var commItems = subscriber.CommunicationMethods.Where(c => c.LookupItem.Value == oldValue).ToList();
                    foreach (var commItem in commItems)
                    {
                        if (newLookupItem != null)
                        {
                            commItem.LookupItem = newLookupItem;
                            commItem.LookupItemId = newLookupItem.Id;
                            modified = true;
                        }
                    }
                    
                    if (modified)
                    {
                        await _subscriberRepository.UpdateAsync(subscriber);
                        updatedCount++;
                    }
                }

                var item = await _lookupRepository.AllEntities.FirstOrDefaultAsync(li => li.Id == id);
                if (item != null)
                {
                    await _lookupRepository.DeleteAsync(item);
                }

                return (true, $"Successfully replaced '{oldValue}' with '{newValue}' for {updatedCount} subscriber(s) and deleted the item!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing and deleting item {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string ErrorMessage, ReplaceAndDeleteInfoDto ReplaceInfo)> PermanentlyDeleteAsync(Guid id, string type)
        {
            try
            {
                if (type == "LookupItem")
                {
                    var item = await _lookupRepository.AllEntities.Include(li => li.Category).FirstOrDefaultAsync(li => li.Id == id);
                    if (item == null) return (false, "Item not found.", null);

                    var countByType = await _subscriberRepository.CountAsync(s => s.Type == item.Value);
                    
                    var activeSubscribers = await _subscriberRepository.Entities
                        .Include(s => s.Interests)
                        .ThenInclude(i => i.LookupItem)
                        .Include(s => s.CommunicationMethods)
                        .ThenInclude(c => c.LookupItem)
                        .ToListAsync();

                    var countByLists = activeSubscribers.Count(s =>
                        s.Interests.Any(i => i.LookupItem.Value == item.Value) ||
                        s.CommunicationMethods.Any(c => c.LookupItem.Value == item.Value));
                    
                    var total = countByType + countByLists;
                    if (total > 0)
                    {
                        var replaceInfo = await GetReplaceAndDeleteInfoAsync(id, item.Category?.Name ?? "", item.Value, item.Label, total);
                        return (false, $"Cannot delete '{item.Label}' - {total} subscriber(s) are using this option.", replaceInfo);
                    }
                    await _lookupRepository.DeleteAsync(item);
                }
                else if (type == "LookupCategory")
                {
                    var category = await _lookupRepository.AllCategories.FirstOrDefaultAsync(lc => lc.Id == id);
                    if (category == null) return (false, "Category not found.", null);
                    
                    var itemCount = await _lookupRepository.GetItemCountByCategoryIdAsync(category.Id);
                    if (itemCount > 0)
                        return (false, $"Cannot delete '{category.Name}' - {itemCount} active item(s) exist in this category.", null);
                    
                    await _lookupRepository.DeleteCategoryAsync(category.Id);
                }
                else if (type == "Subscriber")
                {
                    var sub = await _subscriberRepository.AllEntities.FirstOrDefaultAsync(s => s.Id == id);
                    if (sub != null) await _subscriberRepository.DeleteAsync(sub);
                }
                else if (type == "Newsletter")
                {
                    var news = await _newsletterRepository.AllEntities.FirstOrDefaultAsync(n => n.Id == id);
                    if (news != null) await _newsletterRepository.DeleteAsync(news);
                }
                else
                    return (false, "Invalid item type.", null);

                return (true, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting item {Id}", id);
                return (false, "Unexpected error. Please try again.", null);
            }
        }
    }
}
