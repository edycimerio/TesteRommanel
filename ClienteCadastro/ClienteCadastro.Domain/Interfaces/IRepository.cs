using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Entities;

namespace ClienteCadastro.Domain.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<(IEnumerable<T> Data, int Total)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task RemoveAsync(Guid id);
        Task RemoveAsync(T entity);
        Task<int> SaveChangesAsync();
    }
}
