using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;

namespace NewsletterApp.Application.Interfaces
{
    public interface IUnsubscribeAnalyticsService
    {
        /// <summary>Get paginated unsubscribe history for admin analytics.</summary>
        Task<PagedResult<UnsubscribeHistoryDto>> GetUnsubscribeHistoryPagedAsync(int pageNumber, int pageSize);

        /// <summary>Get counts by reason for charts/dashboard.</summary>
        Task<IReadOnlyList<UnsubscribeStatDto>> GetUnsubscribeStatsAsync();
    }
}
