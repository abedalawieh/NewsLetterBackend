using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;

namespace NewsletterApp.Application.Interfaces
{
    public interface ISubscriberService
    {
        Task<SubscriberResponseDto> CreateSubscriberAsync(CreateSubscriberDto dto);
        Task<SubscriberResponseDto> GetSubscriberByIdAsync(Guid id);
        Task<IEnumerable<SubscriberResponseDto>> GetAllSubscribersAsync();
        Task<SubscriberResponseDto> UpdateSubscriberAsync(Guid id, UpdateSubscriberDto dto);
        Task<bool> DeleteSubscriberAsync(Guid id);
        Task<bool> DeactivateSubscriberAsync(Guid id);
        Task<bool> ActivateSubscriberAsync(Guid id);
        Task<bool> UnsubscribeAsync(string email, string reason, string comment = null);
        Task<PagedResult<SubscriberResponseDto>> GetPagedSubscribersAsync(SubscriberFilterParams filter);
    }
}

