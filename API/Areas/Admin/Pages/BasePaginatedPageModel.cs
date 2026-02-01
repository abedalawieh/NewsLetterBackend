using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewsletterApp.API.Areas.Admin.Pages
{
    /// <summary>
    /// BasePaginatedPageModel provides a foundation for all listing pages.
    /// Follows Single Responsibility Principle by centralizing pagination logic.
    /// Follows Open/Closed Principle by being open for extension via inheritance.
    /// Follows DRY principle by eliminating duplicate pagination properties.
    /// </summary>
    public abstract class BasePaginatedPageModel : PageModel
    {
        protected const int DEFAULT_PAGE_SIZE = 10;

        /// <summary>
        /// Current page number (1-based index)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;

        /// <summary>
        /// Gets or sets the message to display in alerts
        /// </summary>
        public string? SuccessMessage { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public string? WarningMessage { get; protected set; }

        /// <summary>
        /// Sets a success message to be displayed to the user
        /// </summary>
        protected void SetSuccess(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Sets an error message to be displayed to the user
        /// </summary>
        protected void SetError(string message)
        {
            TempData["Error"] = message;
        }

        /// <summary>
        /// Sets a warning message to be displayed to the user
        /// </summary>
        protected void SetWarning(string message)
        {
            TempData["Warning"] = message;
        }

        /// <summary>
        /// Validates and corrects pagination parameters
        /// </summary>
        protected void ValidatePaginationParams()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 5) PageSize = 5;
            if (PageSize > 100) PageSize = 100;
        }

        /// <summary>
        /// Redirect to the same page preserving pagination parameters
        /// </summary>
        protected IActionResult RedirectToSelf()
        {
            return RedirectToPage(new { PageNumber });
        }
    }

    /// <summary>
    /// Base page model for pages with search/filter capabilities
    /// Extends BasePaginatedPageModel with common filter properties
    /// </summary>
    public abstract class BaseFilteredPageModel : BasePaginatedPageModel
    {
        /// <summary>
        /// Search/filter parameters
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public FilterParameters? Filters { get; set; } = new();

        /// <summary>
        /// Available options for type filter
        /// </summary>
        public IEnumerable<FilterOption> AvailableTypes { get; protected set; } = new List<FilterOption>();

        /// <summary>
        /// Available options for interest filter
        /// </summary>
        public IEnumerable<FilterOption> AvailableInterests { get; protected set; } = new List<FilterOption>();
    }

    /// <summary>
    /// Represents filter parameters for pagination
    /// Follows Interface Segregation Principle by being focused and minimal
    /// </summary>
    public class FilterParameters
    {
        public string? SearchTerm { get; set; }
        public string? Type { get; set; }
        public string? Interest { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Represents a filter dropdown option
    /// Simple DTO for rendering in filter components
    /// </summary>
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
