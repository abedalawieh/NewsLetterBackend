using NewsletterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewsletterApp.API.Areas.Admin.Pages.Helpers
{
    /// <summary>
    /// PaginationHelper provides extension methods to apply pagination consistently across all pages.
    /// Follows DRY principle by centralizing pagination logic.
    /// </summary>
    public static class PaginationHelper
    {
        /// <summary>
        /// Creates a paginated result from a list with automatic calculation of pagination properties.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="items">The complete list of items to paginate</param>
        /// <param name="currentPage">Current page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>PagedResult with Items, TotalItems, CurrentPage, and PageSize</returns>
        public static PagedResult<T> Paginate<T>(this IEnumerable<T> items, int currentPage = 1, int pageSize = 10)
        {
            if (currentPage < 1) currentPage = 1;
            if (pageSize < 1) pageSize = 10;

            var itemList = items.ToList();
            var skipCount = (currentPage - 1) * pageSize;

            return new PagedResult<T>
            {
                Items = itemList.Skip(skipCount).Take(pageSize).ToList(),
                TotalItems = itemList.Count,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Creates a paginated result from a queryable with automatic calculation of pagination properties.
        /// </summary>
        /// <typeparam name="T">The type of items in the query</typeparam>
        /// <param name="query">The queryable to paginate</param>
        /// <param name="currentPage">Current page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>PagedResult with Items, TotalItems, CurrentPage, and PageSize</returns>
        public static PagedResult<T> Paginate<T>(this IQueryable<T> query, int currentPage = 1, int pageSize = 10)
        {
            if (currentPage < 1) currentPage = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = query.Count();
            var skipCount = (currentPage - 1) * pageSize;

            return new PagedResult<T>
            {
                Items = query.Skip(skipCount).Take(pageSize).ToList(),
                TotalItems = totalItems,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Validates page number and pagesize, ensuring they meet minimum requirements.
        /// </summary>
        /// <param name="pageNumber">The page number to validate</param>
        /// <param name="pageSize">The page size to validate</param>
        /// <returns>Tuple with validated (pageNumber, pageSize)</returns>
        public static (int PageNumber, int PageSize) ValidatePaginationParams(int pageNumber, int pageSize)
        {
            return (
                Math.Max(pageNumber, 1),
                Math.Clamp(pageSize, 5, 100) // Min 5, Max 100 items per page
            );
        }

        /// <summary>
        /// Creates an empty paginated result (for error cases or empty results).
        /// </summary>
        /// <typeparam name="T">The type of items</typeparam>
        /// <returns>Empty PagedResult</returns>
        public static PagedResult<T> Empty<T>()
        {
            return new PagedResult<T>
            {
                Items = new List<T>(),
                TotalItems = 0,
                CurrentPage = 1,
                PageSize = 10
            };
        }
    }
}
