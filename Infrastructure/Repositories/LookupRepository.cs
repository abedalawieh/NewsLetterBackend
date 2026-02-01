using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Infrastructure.Data;

namespace NewsletterApp.Infrastructure.Repositories
{
    public class LookupRepository : BaseRepository<LookupItem>, ILookupRepository
    {
        public LookupRepository(NewsletterDbContext context) : base(context)
        {
        }

        public IQueryable<LookupCategory> Categories => _context.LookupCategories;
        public IQueryable<LookupCategory> AllCategories => _context.LookupCategories.IgnoreQueryFilters();

        public async Task<IEnumerable<LookupCategory>> GetAllCategoriesAsync()
        {
            return await _context.LookupCategories.Include(c => c.Items).ToListAsync();
        }

        public async Task<LookupCategory> GetCategoryByNameAsync(string name)
        {
            return await _context.LookupCategories.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<LookupItem>> GetItemsByCategoryAsync(string categoryName)
        {
            var category = await GetCategoryByNameAsync(categoryName);
            return category?.Items ?? new List<LookupItem>();
        }

        public async Task<LookupItem> GetItemByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<LookupItem> AddItemAsync(LookupItem item)
        {
            return await AddAsync(item);
        }

        public async Task<LookupItem> UpdateItemAsync(LookupItem item)
        {
            await UpdateAsync(item);
            return item;
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            var item = await GetItemByIdAsync(id);
            if (item == null) return false;
            await DeleteAsync(item);
            return true;
        }

        public async Task<LookupCategory> AddCategoryAsync(LookupCategory category)
        {
            _context.LookupCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateCategoryAsync(LookupCategory category)
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetItemCountByCategoryIdAsync(Guid categoryId)
        {
            return await _context.LookupItems
                .IgnoreQueryFilters()
                .CountAsync(li => li.CategoryId == categoryId && !li.IsDeleted);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _context.LookupCategories.FindAsync(id);
            if (category == null) return false;

            var itemCount = await GetItemCountByCategoryIdAsync(id);
            if (itemCount > 0)
                throw new InvalidOperationException($"Cannot delete category '{category.Name}' - it has {itemCount} item(s). Delete or archive all items first.");
            
            _context.LookupCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
