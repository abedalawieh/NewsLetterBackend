using Microsoft.Extensions.Logging;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;

namespace NewsletterApp.Application.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly INewsletterRepository _newsletterRepository;
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;
        private readonly ILookupRepository _lookupRepository;
        private readonly ILogger<NewsletterService> _logger;

        public NewsletterService(
            INewsletterRepository newsletterRepository, 
            ISubscriberRepository subscriberRepository,
            IEmailService emailService,
            IEmailTemplateService templateService,
            ILookupRepository lookupRepository,
            ILogger<NewsletterService> logger)
        {
            _newsletterRepository = newsletterRepository;
            _subscriberRepository = subscriberRepository;
            _emailService = emailService;
            _templateService = templateService;
            _lookupRepository = lookupRepository;
            _logger = logger;
        }

        public async Task<Newsletter> CreateDraftAsync(string title, string content, List<string> interests)
        {
            try
            {
                var newsletter = Newsletter.Create(
                    title, 
                    content, 
                    string.Join(",", interests)
                );
                return await _newsletterRepository.AddAsync(newsletter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating newsletter draft");
                throw;
            }
        }

        public async Task SendNewsletterAsync(Guid newsletterId, string templateName = null)
        {
            try
            {
                var newsletter = await _newsletterRepository.GetByIdAsync(newsletterId);
                if (newsletter == null || !newsletter.IsDraft)
                {
                    _logger.LogWarning("Newsletter {Id} not found or not a draft", newsletterId);
                    throw new InvalidOperationException("NOT_DRAFT");
                }

                var targetInterests = newsletter.TargetInterests
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                var filteredSubscribers = await _subscriberRepository.GetActiveSubscribersByInterestsAsync(targetInterests);

                _logger.LogInformation(
                    "Sending newsletter {Id} to {Count} subscribers (interests: {Interests})",
                    newsletterId, 
                    filteredSubscribers.Count, 
                    newsletter.TargetInterests);

                if (filteredSubscribers.Count == 0)
                {
                    throw new InvalidOperationException("NO_RECIPIENTS");
                }

                var explicitTemplateHint = !string.IsNullOrWhiteSpace(templateName) ? templateName : newsletter.TemplateName;

                var successCount = 0;
                var failCount = 0;

                foreach (var sub in filteredSubscribers)
                {
                    try
                    {
                    var perRecipientTemplate = _templateService.GetBestTemplateName(
                        explicitTemplateHint,
                        sub.Interests.Select(i => i.LookupItem?.Value ?? string.Empty).Where(v => v.Length > 0));

                    await _emailService.SendNewsletterWithTemplateAsync(
                        sub.Email,
                        sub.FirstName,
                        sub.LastName,
                        sub.Type,
                        sub.CommunicationMethods.Select(m => m.LookupItem?.Value ?? string.Empty).Where(v => v.Length > 0),
                        sub.Interests.Select(i => i.LookupItem?.Value ?? string.Empty).Where(v => v.Length > 0),
                        newsletter.Title,
                        newsletter.Content,
                        perRecipientTemplate
                    );
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        _logger.LogError(ex, "Failed to send email to {Email}", sub.Email);
                    }
                }

                _logger.LogInformation(
                    "Newsletter {Id} dispatch complete. Success: {Success}, Failed: {Failed}",
                    newsletterId, successCount, failCount);

                if (successCount > 0)
                {
                    newsletter.SentAt = DateTime.UtcNow;
                    newsletter.IsDraft = false;
                    newsletter.TemplateName = explicitTemplateHint;
                    await _newsletterRepository.UpdateAsync(newsletter);
                }
                else
                {
                    _logger.LogWarning("Newsletter {Id} was not marked as sent because no emails were delivered.", newsletterId);
                    throw new InvalidOperationException("SEND_FAILED");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending newsletter {Id}", newsletterId);
                throw;
            }
        }

        public async Task<IEnumerable<Newsletter>> GetHistoryAsync()
        {
            try
            {
                return await _newsletterRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting newsletter history");
                throw;
            }
        }

        public async Task<Newsletter> GetByIdAsync(Guid newsletterId)
        {
            try
            {
                return await _newsletterRepository.GetByIdAsync(newsletterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting newsletter {Id}", newsletterId);
                throw;
            }
        }

        public async Task<(IEnumerable<Newsletter> Items, int TotalCount)> GetPagedHistoryAsync(int pageNumber, int pageSize)
        {
            try
            {
                var all = (await _newsletterRepository.GetAllAsync()).ToList();
                var totalCount = all.Count;
                
                var items = all
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged newsletter history");
                throw;
            }
        }

        public async Task<PagedResult<NewsletterListDto>> GetPagedPublishedAsync(NewsletterFilterParams filter)
        {
            try
            {
                var pageNumber = filter?.PageNumber ?? 1;
                var pageSize = filter?.PageSize ?? 6;
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 6;
                if (pageSize > 50) pageSize = 50;

                var interests = (filter?.Interests ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .ToList();

                var (items, totalCount) = await _newsletterRepository.GetPagedPublishedAsync(
                    filter?.SearchTerm,
                    interests,
                    pageNumber,
                    pageSize,
                    filter?.SortBy);

                var interestLookups = await _lookupRepository.GetItemsByCategoryAsync("Interest");
                var labelMap = interestLookups
                    .Where(i => i.IsActive)
                    .GroupBy(i => i.Value, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Label, StringComparer.OrdinalIgnoreCase);

                return new PagedResult<NewsletterListDto>
                {
                    Items = items.Select(n => new NewsletterListDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        TargetInterests = n.TargetInterests,
                        TargetInterestLabels = (n.TargetInterests ?? string.Empty)
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(i => i.Trim())
                            .Where(i => !string.IsNullOrWhiteSpace(i))
                            .Select(i => labelMap.TryGetValue(i, out var label) ? label : i)
                            .ToList(),
                        TemplateName = n.TemplateName,
                        IsDraft = n.IsDraft,
                        SentAt = n.SentAt,
                        CreatedAt = n.CreatedAt
                    }),
                    TotalItems = totalCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged published newsletters");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid newsletterId)
        {
            try
            {
                var newsletter = await _newsletterRepository.GetByIdAsync(newsletterId);
                if (newsletter == null) return false;
                await _newsletterRepository.DeleteAsync(newsletter);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting newsletter {Id}", newsletterId);
                throw;
            }
        }

    }
}
