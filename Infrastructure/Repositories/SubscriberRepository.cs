using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using NewsletterApp.Infrastructure.Data;

namespace NewsletterApp.Infrastructure.Repositories
{
    public class SubscriberRepository : BaseRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(NewsletterDbContext context) : base(context)
        {
        }

        public async Task<Subscriber> GetByEmailAsync(string email)
        {
            return await Entities.FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync()
        {
            return await Entities.Where(s => s.IsActive).ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await Entities.AnyAsync(s => s.Email == email);
        }

        public async Task AddHistoryAsync(SubscriptionHistory history)
        {
            _context.SubscriptionHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Subscriber> Items, int TotalCount)> GetPagedAsync(
            string searchTerm, string type, string interest, bool? isActive, 
            int pageNumber, int pageSize, string sortBy, bool sortDescending)
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
    }
}
