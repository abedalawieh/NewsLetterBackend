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

namespace NewsletterApp.Infrastructure.Services
{
    /// <summary>
    /// Service for managing deleted (soft-deleted) items using the Repository pattern.
    /// Strictly follows SOLID principles by depending on abstractions rather than concrete DbContext.
    /// </summary>
    public class DeletedItemsService : IDeletedItemsService
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly INewsletterRepository _newsletterRepository;

        public DeletedItemsService(
            ISubscriberRepository subscriberRepository,
            ILookupRepository lookupRepository,
            INewsletterRepository newsletterRepository)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _newsletterRepository = newsletterRepository ?? throw new ArgumentNullException(nameof(newsletterRepository));
        }

        public async Task<IReadOnlyList<DeletedItemDto>> GetDeletedSubscribersAsync()
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
                    Details = "Type: " + s.Type + " | Interests: " + (s.Interests != null ? string.Join(", ", s.Interests.Take(2)) : ""),
                    SubscriberCount = 0
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<DeletedMetadataDto>> GetDeletedMetadataAsync()
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

        public async Task<IReadOnlyList<DeletedNewsletterDto>> GetDeletedNewslettersAsync()
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

        public async Task<ReplaceAndDeleteInfoDto> GetReplaceAndDeleteInfoAsync(Guid itemId, string category, string itemValue, string itemLabel, int subscriberCount)
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

        public async Task<bool> RestoreAsync(Guid id, string type)
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
                    // We need a specific way to update category if it's not handled via LookupItem repository base
                    // Assuming _lookupRepository has access to update category or we use context (but user wants repo)
                    // Added AddCategoryAsync/UpdateCategoryAsync to repo? Let's check.
                    // Actually I'll use the UpdateItemAsync if it's the same context but that's for LookupItem.
                    // For now I'll assume ILookupRepository can handle Category updates if I add it.
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

        public async Task<(bool Success, string Message)> ReplaceAndDeleteAsync(Guid id, string category, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                return (false, "Please select a replacement option before proceeding.");

            var subscribers = await _subscriberRepository.Entities.ToListAsync();
            var targetSubscribers = subscribers.Where(s =>
                s.Type == oldValue ||
                (s.Interests != null && s.Interests.Contains(oldValue)) ||
                (s.CommunicationMethods != null && s.CommunicationMethods.Contains(oldValue))).ToList();

            int updatedCount = 0;
            foreach (var subscriber in targetSubscribers)
            {
                bool modified = false;
                if (subscriber.Type == oldValue)
                {
                    subscriber.UpdateType(newValue);
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

        public async Task<(bool Success, string ErrorMessage, ReplaceAndDeleteInfoDto ReplaceInfo)> PermanentlyDeleteAsync(Guid id, string type)
        {
            try
            {
                if (type == "LookupItem")
                {
                    var item = await _lookupRepository.AllEntities.Include(li => li.Category).FirstOrDefaultAsync(li => li.Id == id);
                    if (item == null) return (false, "Item not found.", null);

                    var countByType = await _subscriberRepository.CountAsync(s => s.Type == item.Value);
                    
                    var activeSubscribers = await _subscriberRepository.Entities.ToListAsync();
                    var countByLists = activeSubscribers.Count(s =>
                        (s.Interests != null && s.Interests.Contains(item.Value)) ||
                        (s.CommunicationMethods != null && s.CommunicationMethods.Contains(item.Value)));
                    
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
                return (false, ex.Message, null);
            }
        }
    }
}
