using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Domain.Entities;

namespace NewsletterApp.Application.Interfaces
{
    public interface INewsletterService
    {
        Task<Newsletter> CreateDraftAsync(string title, string content, List<string> interests);
        Task SendNewsletterAsync(Guid newsletterId, string templateName = null);
        Task<IEnumerable<Newsletter>> GetHistoryAsync();
        Task<Newsletter> GetByIdAsync(Guid newsletterId);
        Task<(IEnumerable<Newsletter> Items, int TotalCount)> GetPagedHistoryAsync(int pageNumber, int pageSize);
        Task<PagedResult<NewsletterListDto>> GetPagedPublishedAsync(NewsletterFilterParams filter);
        Task<bool> DeleteAsync(Guid newsletterId);
    }
}
