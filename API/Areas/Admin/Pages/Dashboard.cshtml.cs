using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Infrastructure.Data;
using System;
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
        public List<ActivityRow> RecentActivity { get; set; }
        public IReadOnlyList<UnsubscribeHistoryDto> RecentUnsubscribes { get; set; }

        public class ActivityRow
        {
            public string Action { get; set; }
            public string SubscriberName { get; set; }
            public string Reason { get; set; }
            public DateTime Timestamp { get; set; }
        }


        public async Task OnGetAsync()
        {
            TotalSubscribers = await _context.Subscribers.IgnoreQueryFilters()
                .CountAsync(s => !s.IsDeleted);
            
            ActiveSubscribers = await _context.Subscribers.CountAsync(s => s.IsActive);
            
            NewslettersSent = await _context.Newsletters.CountAsync(n => n.SentAt != null);

            UnsubscribeReasons = await _unsubscribeAnalytics.GetUnsubscribeStatsAsync();

            RecentActivity = await (from h in _context.SubscriptionHistories
                                    join s in _context.Subscribers.IgnoreQueryFilters() on h.SubscriberId equals s.Id into sj
                                    from s in sj.DefaultIfEmpty()
                                    orderby h.Timestamp descending
                                    select new ActivityRow
                                    {
                                        Action = h.Action,
                                        SubscriberName = s == null
                                            ? h.SubscriberId.ToString()
                                            : BuildSubscriberName(s.FirstName, s.LastName, s.Email),
                                        Reason = string.IsNullOrWhiteSpace(h.Reason) ? h.Comment : h.Reason,
                                        Timestamp = h.Timestamp
                                    })
                .Take(5)
                .ToListAsync();

            var paged = await _unsubscribeAnalytics.GetUnsubscribeHistoryPagedAsync(1, 15);
            RecentUnsubscribes = paged.Items?.ToList() ?? new List<UnsubscribeHistoryDto>();
        }

        private static string BuildSubscriberName(string firstName, string lastName, string email)
        {
            var name = $"{firstName} {lastName}".Trim();
            return string.IsNullOrWhiteSpace(name) ? email : name;
        }
    }
}
