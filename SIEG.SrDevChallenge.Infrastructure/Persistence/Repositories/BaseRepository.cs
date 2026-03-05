using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Mongo;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T>(SrDevChallengeContext context) : IRepository<T> where T : class
{
    protected SrDevChallengeContext _context = context;
    protected DbSet<T> _collection = context.Set<T>();
    public IQueryable<T> GetIQueryable()
    {
        return _collection.AsNoTracking().AsQueryable();
    }
    public virtual void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(T));
        _collection.Add(entity);
    }

    public virtual async Task AddAsync(T entity)
    {

        ArgumentNullException.ThrowIfNull(entity, nameof(T));
        await _collection.AddAsync(entity);
        await SaveChangesAsync();

    }

    public virtual void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(T));
        _collection.Remove(entity);
    }

    public virtual async Task DeleteAsync(T entity)
    {
        Delete(entity);
        await SaveChangesAsync();
    }

    public virtual IEnumerable<T> GetByFilter(Expression<Func<T, bool>> filter)
    {
        return _collection.Where(filter).ToList();
    }

    public virtual async Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Where(filter).ToListAsync();
    }

    public virtual void SaveChanges()
    {
        _context.SaveChanges();
    }

    public virtual async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(T));
        _context.Update(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        Update(entity);
        await SaveChangesAsync();
    }
}
