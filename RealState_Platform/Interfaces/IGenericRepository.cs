namespace RealState_Platform.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        // Include deleted (bypass global filter)
        Task<IEnumerable<T>> GetAllIncludingDeletedAsync();
        Task<T> GetByIdIncludingDeletedAsync(int id);
        Task<IEnumerable<T>> FindIncludingDeletedAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountIncludingDeletedAsync(Expression<Func<T, bool>> predicate = null);

        //crud
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }
}
