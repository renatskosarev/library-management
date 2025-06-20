using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using library_management.Data.Interfaces;
using library_management.Models;
using Microsoft.EntityFrameworkCore;
using library_management.Utils;

namespace library_management.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly LibraryDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(LibraryDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // Read operations
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        FileLogger.Log($"Repository<{typeof(T).Name}>.CountAsync() called");
        int result;
        if (predicate == null)
            result = await _dbSet.CountAsync();
        else
            result = await _dbSet.CountAsync(predicate);
        FileLogger.Log($"Repository<{typeof(T).Name}>.CountAsync() result: {result}");
        return result;
    }

    // Create operations
    public virtual async Task<T> AddAsync(T entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    // Update operations
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    // Delete operations
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    // Query operations
    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public virtual IQueryable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }
} 