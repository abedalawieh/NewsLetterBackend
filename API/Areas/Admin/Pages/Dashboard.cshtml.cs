using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly NewsletterDbContext _context;

        public DashboardModel(NewsletterDbContext context)
        {
            _context = context;
        }

        #region stats

        public int TotalSubscribers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int NewslettersSent { get; set; }
        
        public List<UnsubscribeStat> UnsubscribeReasons { get; set; }
        public List<SubscriptionHistory> RecentActivity { get; set; }

        #endregion

        public class UnsubscribeStat
        {
            public string Reason { get; set; }
            public int Count { get; set; }
        }

        public async Task OnGetAsync()
        {
            TotalSubscribers = await _context.Subscribers.IgnoreQueryFilters()
                .CountAsync(s => !s.IsDeleted);
            
            ActiveSubscribers = await _context.Subscribers.CountAsync(s => s.IsActive);
            
            NewslettersSent = await _context.Newsletters.CountAsync(n => n.SentAt != null);

            UnsubscribeReasons = await _context.SubscriptionHistories
                .Where(h => h.Action == "Unsubscribe")
                .GroupBy(h => h.Reason)
                .Select(g => new UnsubscribeStat { Reason = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            RecentActivity = await _context.SubscriptionHistories
                .OrderByDescending(h => h.Timestamp)
                .Take(10)
                .ToListAsync();
        }
    }
}
