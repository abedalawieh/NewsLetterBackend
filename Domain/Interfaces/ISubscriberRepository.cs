using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Domain.Entities;

namespace NewsletterApp.Domain.Interfaces
{
    public interface ISubscriberRepository : IAsyncRepository<Subscriber>
    {
        Task<Subscriber> GetByEmailAsync(string email);
        Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync();
        Task<bool> EmailExistsAsync(string email);
        Task AddHistoryAsync(SubscriptionHistory history);
        Task<(IEnumerable<Subscriber> Items, int TotalCount)> GetPagedAsync(
            string searchTerm, string type, string interest, bool? isActive, 
            int pageNumber, int pageSize, string sortBy, bool sortDescending);
    }
}
