using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace NewsletterApp.Infrastructure.Repositories
{
    public class LookupRepository : BaseRepository<LookupItem>, ILookupRepository
    {
        private readonly ILogger<LookupRepository> _logger;

        public LookupRepository(NewsletterDbContext context, ILogger<BaseRepository<LookupItem>> baseLogger, ILogger<LookupRepository> logger)
            : base(context, baseLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IQueryable<LookupCategory> Categories => _context.LookupCategories;
        public IQueryable<LookupCategory> AllCategories => _context.LookupCategories.IgnoreQueryFilters();

        public async Task<IEnumerable<LookupCategory>> GetAllCategoriesAsync()
        {
            try
            {
                return await _context.LookupCategories.Include(c => c.Items).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all lookup categories");
                throw;
            }
        }

        public async Task<LookupCategory> GetCategoryByNameAsync(string name)
        {
            try
            {
                return await _context.LookupCategories.Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lookup category by name {Name}", name);
                throw;
            }
        }

        public async Task<IEnumerable<LookupItem>> GetItemsByCategoryAsync(string categoryName)
        {
            try
            {
                var category = await GetCategoryByNameAsync(categoryName);
                return category?.Items ?? new List<LookupItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lookup items by category {Category}", categoryName);
                throw;
            }
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
            try
            {
                _context.LookupCategories.Add(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lookup category {Name}", category?.Name);
                throw;
            }
        }

        public async Task<LookupCategory> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                return await _context.LookupCategories
                    .IgnoreQueryFilters()
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lookup category by id {Id}", id);
                throw;
            }
        }

        public async Task UpdateCategoryAsync(LookupCategory category)
        {
            try
            {
                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lookup category {Id}", category?.Id);
                throw;
            }
        }

        public async Task<int> GetItemCountByCategoryIdAsync(Guid categoryId)
        {
            try
            {
                return await _context.LookupItems
                    .IgnoreQueryFilters()
                    .CountAsync(li => li.CategoryId == categoryId && !li.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting lookup items for category {Id}", categoryId);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            try
            {
                var category = await _context.LookupCategories
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == id);
                    
                if (category == null) return false;

                var itemCount = await GetItemCountByCategoryIdAsync(id);
                if (itemCount > 0)
                    throw new InvalidOperationException($"Cannot delete category '{category.Name}' - it has {itemCount} item(s). Delete or archive all items first.");
                
                _context.LookupCategories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lookup category {Id}", id);
                throw;
            }
        }
    }
}
