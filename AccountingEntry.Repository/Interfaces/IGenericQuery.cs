using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AccountingEntry.Repository.Interfaces
{
	public interface IGenericQuery<TEntity> where TEntity : class
	{
        IGenericQuery<TEntity> Clean();
        IGenericQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        IGenericQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector);
        IGenericQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector);
        IGenericQuery<TEntity> Take(int take);
        IGenericQuery<TEntity> ConditionalWhere(Func<bool> condition, Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> SelectAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync();
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    }
}