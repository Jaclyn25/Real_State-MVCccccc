using Microsoft.EntityFrameworkCore;
using RealState_Platform.Data;
using RealState_Platform.Interfaces;
using RealState_Platform.Models;
using System.Linq.Expressions;

namespace RealState_Platform.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return await _dbSet.CountAsync();
            }
            return await _dbSet.Where(predicate).CountAsync();
        }
        // ===== INCLUDING DELETED (Bypass Global Filter) =====

        public async Task<IEnumerable<T>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .ToListAsync();
        }

        public async Task<T> GetByIdIncludingDeletedAsync(int id)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> FindIncludingDeletedAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<int> CountIncludingDeletedAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.IgnoreQueryFilters().CountAsync();
            return await _dbSet.IgnoreQueryFilters().CountAsync(predicate);
        }
        //Crud Operations

        public async Task AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
        }
        public void Update(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
