using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace NewsletterApp.Infrastructure.Repositories
{
    public class SubscriberRepository : BaseRepository<Subscriber>, ISubscriberRepository
    {
        private readonly ILogger<SubscriberRepository> _logger;

        public SubscriberRepository(NewsletterDbContext context, ILogger<BaseRepository<Subscriber>> baseLogger, ILogger<SubscriberRepository> logger)
            : base(context, baseLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Subscriber> GetByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) return null;
                var normalizedEmail = email.Trim().ToLower();
                return await Entities.FirstOrDefaultAsync(s => s.Email.ToLower() == normalizedEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriber by email {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync()
        {
            try
            {
                return await Entities.Where(s => s.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscribers");
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await Entities.AnyAsync(s => s.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence {Email}", email);
                throw;
            }
        }

        public async Task AddHistoryAsync(SubscriptionHistory history)
        {
            try
            {
                _context.SubscriptionHistories.Add(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding subscription history for SubscriberId {SubscriberId}", history?.SubscriberId);
                throw;
            }
        }

        public async Task<(IEnumerable<Subscriber> Items, int TotalCount)> GetPagedAsync(
            string searchTerm, string type, string interest, bool? isActive, 
            int pageNumber, int pageSize, string sortBy, bool sortDescending)
        {
            try
            {
                var query = Entities;

                #region Filtering

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(s => s.FirstName.Contains(searchTerm) || 
                                           s.LastName.Contains(searchTerm) || 
                                           s.Email.Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    query = query.Where(s => s.Type == type);
                }

                if (!string.IsNullOrWhiteSpace(interest))
                {
                    query = query.Where(s => s.Interests.Contains(interest));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }

                #endregion

                var totalCount = await query.CountAsync();

                #region Sorting

                switch (sortBy.ToLower())
                {
                    case "email":
                        query = sortDescending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email);
                        break;
                    case "firstname":
                        query = sortDescending ? query.OrderByDescending(s => s.FirstName) : query.OrderBy(s => s.FirstName);
                        break;
                    case "lastname":
                        query = sortDescending ? query.OrderByDescending(s => s.LastName) : query.OrderBy(s => s.LastName);
                        break;
                    default:
                        query = sortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt);
                        break;
                }

                #endregion

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged subscribers");
                throw;
            }
        }
    }
}
