using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace library_management.Data.Interfaces;

public interface IRepository<T> where T : class
{
    // Read operations
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    
    // Create operations
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    
    // Update operations
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    
    // Delete operations
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    
    // Query operations
    IQueryable<T> Query();
    IQueryable<T> Query(Expression<Func<T, bool>> predicate);
} 