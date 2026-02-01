using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using System;

namespace NewsletterApp.API.Areas.Admin.Pages
{
    public abstract class BasePaginatedPageModel : PageModel
    {
        protected const int DEFAULT_PAGE_SIZE = 10;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;

        public string? SuccessMessage { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public string? WarningMessage { get; protected set; }

        protected void SetSuccess(string message)
        {
            TempData["Success"] = message;
        }

        protected void SetError(string message)
        {
            TempData["Error"] = message;
        }

        protected void SetWarning(string message)
        {
            TempData["Warning"] = message;
        }

        protected void ValidatePaginationParams()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 5) PageSize = 5;
            if (PageSize > 100) PageSize = 100;
        }

        protected PaginationViewModel BuildPagination(int totalItems, string pageParameterName = "pageNumber")
        {
            ValidatePaginationParams();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (PageNumber < 1) PageNumber = 1;
            if (totalPages > 0 && PageNumber > totalPages) PageNumber = totalPages;
            var normalizedTotalPages = Math.Max(1, totalPages);

            return new PaginationViewModel
            {
                CurrentPage = PageNumber,
                TotalPages = normalizedTotalPages,
                TotalItems = totalItems,
                PageSize = PageSize,
                PageParameterName = pageParameterName,
                SelectedPageSize = PageSize
            };
        }

        protected IActionResult RedirectToSelf()
        {
            return RedirectToPage(new { PageNumber });
        }
    }

    public abstract class BaseFilteredPageModel : BasePaginatedPageModel
    {
        [BindProperty(SupportsGet = true)]
        public FilterParameters? Filters { get; set; } = new();

        public IEnumerable<FilterOption> AvailableTypes { get; protected set; } = new List<FilterOption>();

        public IEnumerable<FilterOption> AvailableInterests { get; protected set; } = new List<FilterOption>();
    }

    public class FilterParameters
    {
        public string? SearchTerm { get; set; }
        public string? Type { get; set; }
        public string? Interest { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class FilterOption
    {
        public string Label { get; set; }
        public string Value { get; set; }

        public FilterOption() { }
        public FilterOption(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}
