using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.API.Areas.Admin.Pages.Helpers;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.UnsubscribeAnalytics
{
    public class IndexModel : PageModel
    {
        private readonly IUnsubscribeAnalyticsService _analyticsService;

        public IndexModel(IUnsubscribeAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public PagedResult<UnsubscribeHistoryDto> UnsubscribeHistoryPaged { get; set; }
        public PageHeaderViewModel PageHeader { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        public async Task OnGetAsync(int pageNumber = 1, int pageSize = 25)
        {
            (PageNumber, PageSize) = PaginationHelper.ValidatePaginationParams(pageNumber, pageSize);

            PageHeader = new PageHeaderViewModel
            {
                Title = "Unsubscribe Analytics",
                Subtitle = "See who unsubscribed and why to improve your campaigns.",
                Icon = "fas fa-chart-line"
            };

            UnsubscribeHistoryPaged = await _analyticsService.GetUnsubscribeHistoryPagedAsync(PageNumber, PageSize);
            Pagination = new PaginationViewModel
            {
                CurrentPage = UnsubscribeHistoryPaged.CurrentPage,
                TotalPages = UnsubscribeHistoryPaged.TotalPages,
                TotalItems = UnsubscribeHistoryPaged.TotalItems,
                PageSize = UnsubscribeHistoryPaged.PageSize,
                PageParameterName = "pageNumber",
                SelectedPageSize = PageSize,
                PageSizes = new System.Collections.Generic.List<int> { 10, 25, 50, 100 }
            };
        }
    }
}
