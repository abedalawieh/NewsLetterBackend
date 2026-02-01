using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NewsletterApp.Application.Services
{
    public class SubscriberService : ISubscriberService
    {
        private readonly ISubscriberRepository _repository;
        private readonly ILookupRepository _lookupRepository;
        private readonly ILogger<SubscriberService> _logger;

        public SubscriberService(ISubscriberRepository repository, ILookupRepository lookupRepository, ILogger<SubscriberService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SubscriberResponseDto> CreateSubscriberAsync(CreateSubscriberDto dto)
        {
            try
            {
                var lookupData = await GetLookupDataAsync(dto);
                var existing = await _repository.GetByEmailAsync(dto.Email);
                if (existing != null)
                {
                    if (!existing.IsActive)
                    {
                        existing.Activate();
                        existing.UpdateDetails(
                            dto.FirstName,
                            dto.LastName,
                            dto.Type,
                            lookupData.CommunicationMethods,
                            lookupData.Interests
                        );
                        
                        await _repository.UpdateAsync(existing);
                        await _repository.AddHistoryAsync(SubscriptionHistory.Create(existing.Id, "Activate", "Welcome Back - Resubscribed"));
                        
                        throw new InvalidOperationException($"WELCOME_BACK:{existing.FirstName}");
                    }
                    
                    throw new InvalidOperationException("ALREADY_ACTIVE");
                }

                var subscriber = Subscriber.Create(
                    dto.FirstName,
                    dto.LastName,
                    dto.Email,
                    dto.Type,
                    lookupData.CommunicationMethods,
                    lookupData.Interests
                );

                var created = await _repository.AddAsync(subscriber);
                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscriber");
                throw;
            }
        }

        public async Task<SubscriberResponseDto> GetSubscriberByIdAsync(Guid id)
        {
            try
            {
                var subscriber = await _repository.GetByIdAsync(id);
                if (subscriber == null) throw new KeyNotFoundException($"Subscriber with ID {id} not found.");
                return MapToDto(subscriber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriber by id {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<SubscriberResponseDto>> GetAllSubscribersAsync()
        {
            try
            {
                var subscribers = await _repository.GetAllAsync();
                return subscribers.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subscribers");
                throw;
            }
        }


        public async Task<bool> DeleteSubscriberAsync(Guid id)
        {
            try
            {
                var subscriber = await _repository.GetByIdAsync(id);
                if (subscriber == null) return false;
                
                await _repository.DeleteAsync(subscriber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscriber {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeactivateSubscriberAsync(Guid id)
        {
            try
            {
                var subscriber = await _repository.GetByIdAsync(id);
                if (subscriber == null) return false;
                subscriber.Deactivate();
                await _repository.UpdateAsync(subscriber);
                await _repository.AddHistoryAsync(SubscriptionHistory.Create(id, "Deactivate", "Administrative Action"));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating subscriber {Id}", id);
                throw;
            }
        }

        public async Task<bool> ActivateSubscriberAsync(Guid id)
        {
            try
            {
                var subscriber = await _repository.GetByIdAsync(id);
                if (subscriber == null) return false;
                subscriber.Activate();
                await _repository.UpdateAsync(subscriber);
                await _repository.AddHistoryAsync(SubscriptionHistory.Create(id, "Activate", "Administrative Action"));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating subscriber {Id}", id);
                throw;
            }
        }

        public async Task<bool> UnsubscribeAsync(string email, string reason, string comment = null)
        {
            try
            {
                var subscriber = await _repository.GetByEmailAsync(email);
                if (subscriber == null) 
                {
                    throw new KeyNotFoundException("NO_ACCOUNT");
                }

                if (!subscriber.IsActive)
                {
                     return true;
                }

                subscriber.Deactivate();
                await _repository.UpdateAsync(subscriber);
                await _repository.AddHistoryAsync(SubscriptionHistory.Create(subscriber.Id, "Unsubscribe", reason ?? "User Request", comment));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing {Email}", email);
                throw;
            }
        }

        public async Task<PagedResult<SubscriberResponseDto>> GetPagedSubscribersAsync(SubscriberFilterParams filter)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged subscribers");
                throw;
            }
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
                CommunicationMethods = subscriber.CommunicationMethods.Select(m => m.LookupItem?.Value ?? string.Empty).Where(v => v.Length > 0).ToList(),
                Interests = subscriber.Interests.Select(i => i.LookupItem?.Value ?? string.Empty).Where(v => v.Length > 0).ToList(),
                CreatedAt = subscriber.CreatedAt,
                UpdatedAt = subscriber.UpdatedAt,
                IsActive = subscriber.IsActive
            };
        }

        private async Task<(List<LookupItem> CommunicationMethods, List<LookupItem> Interests)> GetLookupDataAsync(CreateSubscriberDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var typeItems = await _lookupRepository.GetItemsByCategoryAsync("SubscriberType");
            var methodItems = await _lookupRepository.GetItemsByCategoryAsync("CommunicationMethod");
            var interestItems = await _lookupRepository.GetItemsByCategoryAsync("Interest");

            var typeSet = new HashSet<string>(
                typeItems.Where(i => i.IsActive).Select(i => i.Value),
                StringComparer.OrdinalIgnoreCase
            );
            var methodSet = new HashSet<string>(
                methodItems.Where(i => i.IsActive).Select(i => i.Value),
                StringComparer.OrdinalIgnoreCase
            );
            var interestSet = new HashSet<string>(
                interestItems.Where(i => i.IsActive).Select(i => i.Value),
                StringComparer.OrdinalIgnoreCase
            );

            if (string.IsNullOrWhiteSpace(dto.Type) || !typeSet.Contains(dto.Type))
            {
                throw new ArgumentException("Invalid subscriber type");
            }

            if (dto.CommunicationMethods == null || dto.CommunicationMethods.Count == 0)
            {
                throw new ArgumentException("At least one communication method is required");
            }

            if (dto.CommunicationMethods.Any(m => string.IsNullOrWhiteSpace(m) || !methodSet.Contains(m)))
            {
                throw new ArgumentException("Invalid communication method");
            }

            if (dto.Interests == null || dto.Interests.Count == 0)
            {
                throw new ArgumentException("At least one interest is required");
            }

            if (dto.Interests.Any(i => string.IsNullOrWhiteSpace(i) || !interestSet.Contains(i)))
            {
                throw new ArgumentException("Invalid interest");
            }

            var methodMap = methodItems.Where(i => i.IsActive).ToDictionary(i => i.Value, i => i, StringComparer.OrdinalIgnoreCase);
            var interestMap = interestItems.Where(i => i.IsActive).ToDictionary(i => i.Value, i => i, StringComparer.OrdinalIgnoreCase);

            var methods = dto.CommunicationMethods.Select(m => methodMap[m]).ToList();
            var interests = dto.Interests.Select(i => interestMap[i]).ToList();
            return (methods, interests);
        }
    }
}
