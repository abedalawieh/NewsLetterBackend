using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly INewsletterRepository _newsletterRepository;
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IEmailService _emailService;

        public NewsletterService(
            INewsletterRepository newsletterRepository, 
            ISubscriberRepository subscriberRepository,
            IEmailService emailService)
        {
            _newsletterRepository = newsletterRepository;
            _subscriberRepository = subscriberRepository;
            _emailService = emailService;
        }

        public async Task<Newsletter> CreateDraftAsync(string title, string content, List<string> interests)
        {
            var newsletter = Newsletter.Create(title, content, string.Join(",", interests));
            return await _newsletterRepository.AddAsync(newsletter);
        }

        public async Task SendNewsletterAsync(Guid newsletterId)
        {
            var newsletter = await _newsletterRepository.GetByIdAsync(newsletterId);
            if (newsletter == null || !newsletter.IsDraft) return;

            var targetInterests = newsletter.TargetInterests.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            var subscribers = await _subscriberRepository.GetActiveSubscribersAsync();

            var filteredSubscribers = subscribers
                .Where(s => s.Interests.Any(i => targetInterests.Contains(i)))
                .ToList();

            #region Email Dispatch

            foreach (var sub in filteredSubscribers)
            {
                try 
                {
                    await _emailService.SendNewsletterAsync(sub.Email, newsletter.Title, newsletter.Content);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other subscribers
                    Console.WriteLine($"Failed to send email to {sub.Email}: {ex.Message}");
                }
            }

            #endregion

            newsletter.SentAt = DateTime.UtcNow;
            newsletter.IsDraft = false;
            await _newsletterRepository.UpdateAsync(newsletter);
        }

        public async Task<IEnumerable<Newsletter>> GetHistoryAsync()
        {
            return await _newsletterRepository.GetAllAsync();
        }

        public async Task<bool> DeleteAsync(Guid newsletterId)
        {
            var newsletter = await _newsletterRepository.GetByIdAsync(newsletterId);
            if (newsletter == null) return false;
            await _newsletterRepository.DeleteAsync(newsletter);
            return true;
        }
    }
}
