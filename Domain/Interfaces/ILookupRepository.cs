using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Domain.Entities;

namespace NewsletterApp.Domain.Interfaces
{
    public interface ILookupRepository : IAsyncRepository<LookupItem>
    {
        Task<IEnumerable<LookupCategory>> GetAllCategoriesAsync();
        Task<LookupCategory> GetCategoryByNameAsync(string name);
        Task<IEnumerable<LookupItem>> GetItemsByCategoryAsync(string categoryName);
        Task<LookupItem> GetItemByIdAsync(Guid id);
        Task<LookupItem> AddItemAsync(LookupItem item);
        Task<LookupItem> UpdateItemAsync(LookupItem item);
        Task<bool> DeleteItemAsync(Guid id);
        
        // New methods for Category management
        Task<LookupCategory> AddCategoryAsync(LookupCategory category);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<int> GetItemCountByCategoryIdAsync(Guid categoryId);
    }
}
