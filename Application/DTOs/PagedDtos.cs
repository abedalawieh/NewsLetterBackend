using System.Collections.Generic;

namespace NewsletterApp.Application.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class SubscriberFilterParams
    {
        public string SearchTerm { get; set; }
        public string Type { get; set; }
        public string Interest { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class NewsletterFilterParams
    {
        public string? SearchTerm { get; set; }
        public string? Interests { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public string? SortBy { get; set; } = "newest";
    }
}
