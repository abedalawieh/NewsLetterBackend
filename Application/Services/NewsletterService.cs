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
                .Where(s => s.CommunicationMethods != null && s.CommunicationMethods.Any(cm => 
                    "Email".Equals(cm, StringComparison.OrdinalIgnoreCase)))
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

            // Use newsletter-level template as an explicit hint, but choose template per recipient
            var explicitTemplateHint = !string.IsNullOrWhiteSpace(templateName) ? templateName : newsletter.TemplateName;

            #region Email Dispatch

            var successCount = 0;
            var failCount = 0;

            foreach (var sub in filteredSubscribers)
            {
                try
                {
                    var perRecipientTemplate = _templateService.GetBestTemplateName(
                        explicitTemplateHint,
                        sub.Type,
                        sub.Interests);

                    await _emailService.SendNewsletterWithTemplateAsync(
                        sub.Email,
                        sub.FirstName,
                        sub.LastName,
                        sub.Type,
                        sub.CommunicationMethods ?? Enumerable.Empty<string>(),
                        sub.Interests ?? Enumerable.Empty<string>(),
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

            #endregion

            // Only mark the newsletter as sent when at least one email was delivered.
            if (successCount > 0)
            {
                newsletter.SentAt = DateTime.UtcNow;
                newsletter.IsDraft = false;
                // Persist the newsletter-level template hint if present
                newsletter.TemplateName = explicitTemplateHint;
                await _newsletterRepository.UpdateAsync(newsletter);
            }
            else
            {
                _logger.LogWarning("Newsletter {Id} was not marked as sent because no emails were delivered.", newsletterId);
                throw new InvalidOperationException("No emails were sent for the newsletter. Check SMTP settings and templates.");
            }
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

        public async Task<string> RenderTemplateForRecipientAsync(Guid newsletterId, Guid subscriberId, string templateName = null)
        {
            var newsletter = await _newsletterRepository.GetByIdAsync(newsletterId);
            if (newsletter == null) throw new KeyNotFoundException("Newsletter not found");

            var subscriber = await _subscriberRepository.GetByIdAsync(subscriberId);
            if (subscriber == null) throw new KeyNotFoundException("Subscriber not found");

            var targetInterests = newsletter.TargetInterests
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var explicitTemplateHint = !string.IsNullOrWhiteSpace(templateName) ? templateName : newsletter.TemplateName;

            var perRecipientTemplate = _templateService.GetBestTemplateName(
                explicitTemplateHint,
                subscriber.Type,
                subscriber.Interests);

            var unsubscribeLink = $"http://localhost:5173/unsubscribe?email={Uri.EscapeDataString(subscriber.Email)}";

            var context = new Dictionary<string, string>
            {
                { "Subject", newsletter.Title },
                { "FirstName", subscriber.FirstName ?? "Subscriber" },
                { "LastName", subscriber.LastName ?? string.Empty },
                { "Type", subscriber.Type ?? string.Empty },
                { "CommunicationMethods", string.Join(", ", subscriber.CommunicationMethods ?? new List<string>()) },
                { "Interests", string.Join(", ", subscriber.Interests ?? new List<string>()) },
                { "Content", newsletter.Content },
                { "UnsubscribeLink", unsubscribeLink },
                { "Year", DateTime.Now.Year.ToString() }
            };

            // Use the template service to render HTML for preview
            var html = await _templateService.RenderTemplateAsync(perRecipientTemplate, context);
            return html;
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
