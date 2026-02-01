using NewsletterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewsletterApp.API.Areas.Admin.Pages.Helpers
{
    public static class PaginationHelper
    {
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

        public static (int PageNumber, int PageSize) ValidatePaginationParams(int pageNumber, int pageSize)
        {
            return (
                Math.Max(pageNumber, 1),
                Math.Clamp(pageSize, 5, 100) // Min 5, Max 100 items per page
            );
        }

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
