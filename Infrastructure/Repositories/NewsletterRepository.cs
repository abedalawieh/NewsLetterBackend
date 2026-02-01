using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Infrastructure.Repositories
{
    public class NewsletterRepository : BaseRepository<Newsletter>, INewsletterRepository
    {
        private readonly ILogger<NewsletterRepository> _logger;

        public NewsletterRepository(NewsletterDbContext context, ILogger<BaseRepository<Newsletter>> baseLogger, ILogger<NewsletterRepository> logger)
            : base(context, baseLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(IEnumerable<Newsletter> Items, int TotalCount)> GetPagedPublishedAsync(string searchTerm, IEnumerable<string> interests, int pageNumber, int pageSize, string sortBy)
        {
            try
            {
                var query = Entities.AsQueryable();

                query = query.Where(n => !n.IsDraft);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var normalized = searchTerm.Trim();
                    var condensed = normalized.Replace(" ", "");
                    query = query.Where(n =>
                        n.Title.Contains(normalized) ||
                        n.TargetInterests.Contains(normalized) ||
                        n.TargetInterests.Contains(condensed));
                }

                var interestList = interests?
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Select(i => i.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                foreach (var interest in interestList)
                {
                    query = query.Where(n => n.TargetInterests.Contains(interest));
                }

                var totalCount = await query.CountAsync();

                switch (sortBy?.ToLowerInvariant())
                {
                    case "oldest":
                        query = query.OrderBy(n => n.SentAt ?? n.CreatedAt);
                        break;
                    case "title":
                        query = query.OrderBy(n => n.Title);
                        break;
                    default:
                        query = query.OrderByDescending(n => n.SentAt ?? n.CreatedAt);
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
                _logger.LogError(ex, "Error getting paged newsletters");
                throw;
            }
        }
    }
}
