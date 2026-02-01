using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;

namespace NewsletterApp.Application.Services
{
    public class SubscriberService : ISubscriberService
    {
        private readonly ISubscriberRepository _repository;

        public SubscriberService(ISubscriberRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<SubscriberResponseDto> CreateSubscriberAsync(CreateSubscriberDto dto)
        {
            if (await _repository.EmailExistsAsync(dto.Email))
            {
                throw new InvalidOperationException($"A subscriber with email {dto.Email} already exists.");
            }

            var subscriber = Subscriber.Create(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Type,
                dto.CommunicationMethods,
                dto.Interests
            );

            var created = await _repository.AddAsync(subscriber);
            return MapToDto(created);
        }

        public async Task<SubscriberResponseDto> GetSubscriberByIdAsync(Guid id)
        {
            var subscriber = await _repository.GetByIdAsync(id);
            if (subscriber == null) throw new KeyNotFoundException($"Subscriber with ID {id} not found.");
            return MapToDto(subscriber);
        }

        public async Task<IEnumerable<SubscriberResponseDto>> GetAllSubscribersAsync()
        {
            var subscribers = await _repository.GetAllAsync();
            return subscribers.Select(MapToDto);
        }

        public async Task<SubscriberResponseDto> UpdateSubscriberAsync(Guid id, UpdateSubscriberDto dto)
        {
            var subscriber = await _repository.GetByIdAsync(id);
            if (subscriber == null) throw new KeyNotFoundException($"Subscriber with ID {id} not found.");

            // Update mapping logic here if needed
            
            await _repository.UpdateAsync(subscriber);
            return MapToDto(subscriber);
        }

        public async Task<bool> DeleteSubscriberAsync(Guid id)
        {
            var subscriber = await _repository.GetByIdAsync(id);
            if (subscriber == null) return false;
            
            await _repository.DeleteAsync(subscriber);
            return true;
        }

        public async Task<bool> DeactivateSubscriberAsync(Guid id)
        {
            var subscriber = await _repository.GetByIdAsync(id);
            if (subscriber == null) return false;
            subscriber.Deactivate();
            await _repository.UpdateAsync(subscriber);
            await _repository.AddHistoryAsync(SubscriptionHistory.Create(id, "Deactivate", "Administrative Action"));
            return true;
        }

        public async Task<bool> ActivateSubscriberAsync(Guid id)
        {
            var subscriber = await _repository.GetByIdAsync(id);
            if (subscriber == null) return false;
            subscriber.Activate();
            await _repository.UpdateAsync(subscriber);
            await _repository.AddHistoryAsync(SubscriptionHistory.Create(id, "Activate", "Administrative Action"));
            return true;
        }

        public async Task<bool> UnsubscribeAsync(string email, string reason)
        {
            var subscriber = await _repository.GetByEmailAsync(email);
            if (subscriber == null) return false;

            subscriber.Deactivate();
            await _repository.UpdateAsync(subscriber);
            await _repository.AddHistoryAsync(SubscriptionHistory.Create(subscriber.Id, "Unsubscribe", reason));
            return true;
        }

        public async Task<PagedResult<SubscriberResponseDto>> GetPagedSubscribersAsync(SubscriberFilterParams filter)
        {
            var (items, totalCount) = await _repository.GetPagedAsync(
                filter.SearchTerm,
                filter.Type,
                filter.Interest,
                filter.IsActive,
                filter.PageNumber,
                filter.PageSize,
                filter.SortBy,
                filter.SortDescending
            );

            return new PagedResult<SubscriberResponseDto>
            {
                Items = items.Select(MapToDto),
                TotalItems = totalCount,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        private SubscriberResponseDto MapToDto(Subscriber subscriber)
        {
            return new SubscriberResponseDto
            {
                Id = subscriber.Id,
                FirstName = subscriber.FirstName,
                LastName = subscriber.LastName,
                Email = subscriber.Email,
                Type = subscriber.Type,
                CommunicationMethods = subscriber.CommunicationMethods,
                Interests = subscriber.Interests,
                CreatedAt = subscriber.CreatedAt,
                UpdatedAt = subscriber.UpdatedAt,
                IsActive = subscriber.IsActive
            };
        }
    }
}
