using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;

namespace NewsletterApp.Application.Interfaces
{
    public interface IDeletedItemsService
    {
        Task<IReadOnlyList<DeletedItemDto>> GetDeletedSubscribersAsync();
        Task<IReadOnlyList<DeletedMetadataDto>> GetDeletedMetadataAsync();
        Task<IReadOnlyList<DeletedNewsletterDto>> GetDeletedNewslettersAsync();
        Task<ReplaceAndDeleteInfoDto> GetReplaceAndDeleteInfoAsync(Guid itemId, string category, string itemValue, string itemLabel, int subscriberCount);
        Task<bool> RestoreAsync(Guid id, string type);
        Task<(bool Success, string Message)> ReplaceAndDeleteAsync(Guid id, string category, string oldValue, string newValue);
        /// <summary>Returns (Success, ErrorMessage, ReplaceInfo). When Success is false and ReplaceInfo is not null, show replace-and-delete form.</summary>
        Task<(bool Success, string ErrorMessage, ReplaceAndDeleteInfoDto ReplaceInfo)> PermanentlyDeleteAsync(Guid id, string type);
    }
}
