using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NewsletterApp.Infrastructure.Repositories
{
    public class BaseRepository<T> : IAsyncRepository<T> where T : class
    {
        protected readonly NewsletterDbContext _context;
        private readonly ILogger<BaseRepository<T>> _logger;

        public BaseRepository(NewsletterDbContext context, ILogger<BaseRepository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual IQueryable<T> Entities => _context.Set<T>();
        public virtual IQueryable<T> AllEntities => _context.Set<T>().IgnoreQueryFilters();

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {Entity} by id {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            try
            {
                return await _context.Set<T>().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {Entity}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _context.Set<T>().Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {Entity} by predicate", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                await _context.Set<T>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {Entity}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task UpdateAsync(T entity)
        {
            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {Entity}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {Entity}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                if (predicate != null)
                    return await _context.Set<T>().CountAsync(predicate);
                return await _context.Set<T>().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting {Entity}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                if (predicate != null)
                    return await _context.Set<T>().AnyAsync(predicate);
                return await _context.Set<T>().AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking any {Entity}", typeof(T).Name);
                throw;
            }
        }
    }
}
