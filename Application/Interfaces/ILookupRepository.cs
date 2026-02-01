using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsletterApp.Domain.Entities;

namespace NewsletterApp.Application.Interfaces
{
    public interface ILookupRepository : IAsyncRepository<LookupItem>
    {
        IQueryable<LookupCategory> Categories { get; }
        IQueryable<LookupCategory> AllCategories { get; }
        Task<IEnumerable<LookupCategory>> GetAllCategoriesAsync();
        Task<LookupCategory> GetCategoryByIdAsync(Guid id);
        Task<LookupCategory> GetCategoryByNameAsync(string name);
        Task<IEnumerable<LookupItem>> GetItemsByCategoryAsync(string categoryName);
        Task<LookupItem> GetItemByIdAsync(Guid id);
        Task<LookupItem> AddItemAsync(LookupItem item);
        Task<LookupItem> UpdateItemAsync(LookupItem item);
        Task<bool> DeleteItemAsync(Guid id);
        
        Task<LookupCategory> AddCategoryAsync(LookupCategory category);
        Task UpdateCategoryAsync(LookupCategory category);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<int> GetItemCountByCategoryIdAsync(Guid categoryId);
    }
}
