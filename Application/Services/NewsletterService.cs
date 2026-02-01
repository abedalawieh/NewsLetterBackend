using Microsoft.Extensions.Logging;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Services
{
    /// <summary>
    /// Newsletter Service implementation
    /// Follows Single Responsibility Principle - manages newsletter lifecycle
    /// Uses Strategy pattern for template selection
    /// </summary>
    public class NewsletterService : INewsletterService
    {
        private readonly INewsletterRepository _newsletterRepository;
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;
        private readonly ILogger<NewsletterService> _logger;

        public NewsletterService(
            INewsletterRepository newsletterRepository, 
            ISubscriberRepository subscriberRepository,
            IEmailService emailService,
            IEmailTemplateService templateService,
            ILogger<NewsletterService> logger)
        {
            _newsletterRepository = newsletterRepository;
            _subscriberRepository = subscriberRepository;
            _emailService = emailService;
            _templateService = templateService;
            _logger = logger;
        }

        public async Task<Newsletter> CreateDraftAsync(string title, string content, List<string> interests)
        {
            return await CreateDraftAsync(title, content, interests, null);
        }

        public async Task<Newsletter> CreateDraftAsync(string title, string content, List<string> interests, string targetSubscriberType)
        {
            var newsletter = Newsletter.Create(
                title, 
                content, 
                string.Join(",", interests),
                targetSubscriberType
            );
            return await _newsletterRepository.AddAsync(newsletter);
        }

        public async Task SendNewsletterAsync(Guid newsletterId, string templateName = null, string targetSubscriberType = null)
        {
            var newsletter = await _newsletterRepository.GetByIdAsync(newsletterId);
            if (newsletter == null || !newsletter.IsDraft)
            {
                _logger.LogWarning("Newsletter {Id} not found or not a draft", newsletterId);
                return;
            }

            var targetInterests = newsletter.TargetInterests
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            // Get subscribers filtered by interests
            var subscribers = await _subscriberRepository.GetActiveSubscribersAsync();

            var filteredSubscribers = subscribers
                .Where(s => s.Interests.Any(i => targetInterests.Contains(i, StringComparer.OrdinalIgnoreCase)))
                .ToList();

            // Apply subscriber type filter if specified (from parameter or newsletter setting)
            var effectiveSubscriberType = targetSubscriberType ?? newsletter.TargetSubscriberType;
            if (!string.IsNullOrWhiteSpace(effectiveSubscriberType))
            {
                filteredSubscribers = filteredSubscribers
                    .Where(s => s.Type.Equals(effectiveSubscriberType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            _logger.LogInformation(
                "Sending newsletter {Id} to {Count} subscribers (interests: {Interests}, type: {Type})",
                newsletterId, 
                filteredSubscribers.Count, 
                newsletter.TargetInterests,
                effectiveSubscriberType ?? "All");

            // Determine template to use
            var effectiveTemplate = DetermineTemplate(templateName, newsletter, targetInterests);

            #region Email Dispatch

            var successCount = 0;
            var failCount = 0;

            foreach (var sub in filteredSubscribers)
            {
                try 
                {
                    await _emailService.SendNewsletterWithTemplateAsync(
                        sub.Email, 
                        sub.FirstName,
                        newsletter.Title, 
                        newsletter.Content,
                        effectiveTemplate
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

            #endregion

            newsletter.SentAt = DateTime.UtcNow;
            newsletter.IsDraft = false;
            newsletter.TemplateName = effectiveTemplate;
            await _newsletterRepository.UpdateAsync(newsletter);
        }

        public async Task<IEnumerable<Newsletter>> GetHistoryAsync()
        {
            return await _newsletterRepository.GetAllAsync();
        }

        public async Task<Newsletter> GetByIdAsync(Guid newsletterId)
        {
            return await _newsletterRepository.GetByIdAsync(newsletterId);
        }

        public async Task<(IEnumerable<Newsletter> Items, int TotalCount)> GetPagedHistoryAsync(int pageNumber, int pageSize)
        {
            var all = (await _newsletterRepository.GetAllAsync()).ToList();
            var totalCount = all.Count;
            
            var items = all
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return (items, totalCount);
        }

        public async Task<bool> DeleteAsync(Guid newsletterId)
        {
            var newsletter = await _newsletterRepository.GetByIdAsync(newsletterId);
            if (newsletter == null) return false;
            await _newsletterRepository.DeleteAsync(newsletter);
            return true;
        }

        /// <summary>
        /// Determines the appropriate template based on newsletter settings and interests
        /// Implements Strategy pattern for template selection
        /// </summary>
        private string DetermineTemplate(string explicitTemplate, Newsletter newsletter, List<string> interests)
        {
            // Priority 1: Explicit template parameter
            if (!string.IsNullOrWhiteSpace(explicitTemplate) && _templateService.TemplateExists(explicitTemplate))
            {
                return explicitTemplate;
            }

            // Priority 2: Template stored in newsletter
            if (!string.IsNullOrWhiteSpace(newsletter.TemplateName) && _templateService.TemplateExists(newsletter.TemplateName))
            {
                return newsletter.TemplateName;
            }

            // Priority 3: Template based on subscriber type
            if (!string.IsNullOrWhiteSpace(newsletter.TargetSubscriberType))
            {
                var typeTemplate = _templateService.GetTemplateNameForSubscriberType(newsletter.TargetSubscriberType);
                if (_templateService.TemplateExists(typeTemplate))
                {
                    return typeTemplate;
                }
            }

            // Priority 4: Template based on single interest
            if (interests.Count == 1)
            {
                var interestTemplate = _templateService.GetTemplateNameForInterest(interests[0]);
                if (_templateService.TemplateExists(interestTemplate))
                {
                    return interestTemplate;
                }
            }

            // Priority 5: Fall back to generic template for multiple interests
            return "GenericNewsletter";
        }
    }
}
