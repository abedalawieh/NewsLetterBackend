using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;

namespace NewsletterApp.Application.Interfaces
{
    public interface IUnsubscribeAnalyticsService
    {
        Task<PagedResult<UnsubscribeHistoryDto>> GetUnsubscribeHistoryPagedAsync(int pageNumber, int pageSize);

        Task<IReadOnlyList<UnsubscribeStatDto>> GetUnsubscribeStatsAsync();
    }
}
