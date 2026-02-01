using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
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
        private readonly IUnsubscribeAnalyticsService _unsubscribeAnalytics;

        public DashboardModel(NewsletterDbContext context, IUnsubscribeAnalyticsService unsubscribeAnalytics)
        {
            _context = context;
            _unsubscribeAnalytics = unsubscribeAnalytics;
        }


        public int TotalSubscribers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int NewslettersSent { get; set; }
        
        public IReadOnlyList<UnsubscribeStatDto> UnsubscribeReasons { get; set; }
        public List<SubscriptionHistory> RecentActivity { get; set; }
        public IReadOnlyList<UnsubscribeHistoryDto> RecentUnsubscribes { get; set; }


        public async Task OnGetAsync()
        {
            TotalSubscribers = await _context.Subscribers.IgnoreQueryFilters()
                .CountAsync(s => !s.IsDeleted);
            
            ActiveSubscribers = await _context.Subscribers.CountAsync(s => s.IsActive);
            
            NewslettersSent = await _context.Newsletters.CountAsync(n => n.SentAt != null);

            UnsubscribeReasons = await _unsubscribeAnalytics.GetUnsubscribeStatsAsync();

            RecentActivity = await _context.SubscriptionHistories
                .OrderByDescending(h => h.Timestamp)
                .Take(10)
                .ToListAsync();

            var paged = await _unsubscribeAnalytics.GetUnsubscribeHistoryPagedAsync(1, 15);
            RecentUnsubscribes = paged.Items?.ToList() ?? new List<UnsubscribeHistoryDto>();
        }
    }
}
