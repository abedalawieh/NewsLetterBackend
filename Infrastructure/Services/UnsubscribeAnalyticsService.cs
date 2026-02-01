using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Infrastructure.Data;

namespace NewsletterApp.Infrastructure.Services
{
    public class UnsubscribeAnalyticsService : IUnsubscribeAnalyticsService
    {
        private readonly NewsletterDbContext _context;

        public UnsubscribeAnalyticsService(NewsletterDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UnsubscribeHistoryDto>> GetUnsubscribeHistoryPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            pageSize = System.Math.Min(pageSize, 100);

            var baseQuery = _context.SubscriptionHistories
                .Where(h => h.Action == "Unsubscribe")
                .OrderByDescending(h => h.Timestamp)
                .Join(
                    _context.Subscribers.IgnoreQueryFilters(),
                    h => h.SubscriberId,
                    s => s.Id,
                    (h, s) => new UnsubscribeHistoryDto
                    {
                        Id = h.Id,
                        Email = s.Email,
                        Reason = h.Reason ?? "",
                        Comment = h.Comment,
                        Timestamp = h.Timestamp
                    });

            var totalItems = await baseQuery.CountAsync();
            var items = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<UnsubscribeHistoryDto>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<UnsubscribeStatDto>> GetUnsubscribeStatsAsync()
        {
            var list = await _context.SubscriptionHistories
                .Where(h => h.Action == "Unsubscribe")
                .GroupBy(h => h.Reason ?? "(empty)")
                .Select(g => new UnsubscribeStatDto { Reason = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
            return list;
        }
    }
}
