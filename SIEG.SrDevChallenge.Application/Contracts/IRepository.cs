using System;
using System.Linq.Expressions;

namespace SIEG.SrDevChallenge.Application.Contracts;

public interface IRepository<T>
{
    IEnumerable<T> GetByFilter(Expression<Func<T, bool>> filter);
    Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter);
    IQueryable<T> GetIQueryable();
    void Add(T entity);
    Task AddAsync(T entity);
    void Update(T entity);
    Task UpdateAsync(T entity);
    void Delete(T entity);
    Task DeleteAsync(T entity);
    void SaveChanges();
    Task SaveChangesAsync();
}
