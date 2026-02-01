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

        public override async Task<Subscriber> GetByIdAsync(Guid id)
        {
            try
            {
                return await Entities
                    .Include(s => s.CommunicationMethods)
                    .ThenInclude(sm => sm.LookupItem)
                    .Include(s => s.Interests)
                    .ThenInclude(si => si.LookupItem)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriber by id {Id}", id);
                throw;
            }
        }

        public override async Task<IReadOnlyList<Subscriber>> GetAllAsync()
        {
            try
            {
                return await Entities
                    .Include(s => s.CommunicationMethods)
                    .ThenInclude(sm => sm.LookupItem)
                    .Include(s => s.Interests)
                    .ThenInclude(si => si.LookupItem)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscribers");
                throw;
            }
        }

        public async Task<Subscriber> GetByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) return null;
                var normalizedEmail = email.Trim().ToLower();
                return await Entities
                    .Include(s => s.CommunicationMethods)
                    .ThenInclude(sm => sm.LookupItem)
                    .Include(s => s.Interests)
                    .ThenInclude(si => si.LookupItem)
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == normalizedEmail);
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
                return await Entities
                    .Where(s => s.IsActive)
                    .Include(s => s.CommunicationMethods)
                    .ThenInclude(sm => sm.LookupItem)
                    .Include(s => s.Interests)
                    .ThenInclude(si => si.LookupItem)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscribers");
                throw;
            }
        }

        public async Task<List<Subscriber>> GetActiveSubscribersByInterestsAsync(IEnumerable<string> interests)
        {
            try
            {
                var interestList = interests?
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Select(i => i.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (interestList.Count == 0)
                {
                    return new List<Subscriber>();
                }

                var query = Entities
                    .Where(s => s.IsActive)
                    .Where(s => s.CommunicationMethods.Any(cm => cm.LookupItem.Value == "Email"))
                    .Where(s => s.Interests.Any(i => interestList.Contains(i.LookupItem.Value)))
                    .Include(s => s.CommunicationMethods)
                    .ThenInclude(sm => sm.LookupItem)
                    .Include(s => s.Interests)
                    .ThenInclude(si => si.LookupItem);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscribers by interests");
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
                    query = query.Where(s => s.Interests.Any(i => i.LookupItem.Value == interest));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }


                var totalCount = await query.CountAsync();


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
