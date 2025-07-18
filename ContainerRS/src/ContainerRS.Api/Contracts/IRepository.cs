﻿using System.Linq.Expressions;

namespace ContainerRS.Api.Contracts
{
    public interface IRepository<T>
    {
        Task<T> AddAsync(T obj, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T obj, CancellationToken cancellationToken = default);
        Task RemoveAsync(T obj, CancellationToken cancellationToken = default);
        Task<T?> GetFirstAsync<TProperty>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TProperty>> orderBy,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetWhereAsync(
            Expression<Func<T, bool>>? filtro = default,
            CancellationToken cancellationToken = default);
            
    }
}
