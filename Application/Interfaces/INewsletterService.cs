using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Domain.Entities;

namespace NewsletterApp.Application.Interfaces
{
    public interface INewsletterService
    {
        Task<Newsletter> CreateDraftAsync(string title, string content, List<string> interests);
        Task SendNewsletterAsync(Guid newsletterId);
        Task<IEnumerable<Newsletter>> GetHistoryAsync();
        Task<bool> DeleteAsync(Guid newsletterId);
    }
}
