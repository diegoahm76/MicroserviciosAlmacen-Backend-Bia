using AccountingEntry.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AccountingEntry.Repository
{
	public class GenericQuery<TEntity> : IGenericQuery<TEntity> where TEntity : class
	{
        private int? _skip;
        private int? _starterSkip;
        private int? _take;
        private int? _starterTake;
        private IQueryable<TEntity> _query;
        private IQueryable<TEntity> _starterQuery;
        private IOrderedQueryable<TEntity> _orderedQuery;
        private IOrderedQueryable<TEntity> _starterOrderedQuery;

        public GenericQuery(IGenericRepository<TEntity> repository)
        {
            _query = repository.Queryable();
            _starterQuery = repository.Queryable();
            _starterSkip = _skip;
            _starterTake = _take;
            _starterOrderedQuery = _orderedQuery;
        }

        public virtual IGenericQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
            => Set(q => q._query = q._query.Where(predicate));

        public virtual IGenericQuery<TEntity> Clean()
        {
            Set(q => q._skip = q._starterSkip);
            Set(q => q._take = q._starterTake);
            Set(q => q._orderedQuery = q._starterOrderedQuery);
            return Set(q => q._query = q._starterQuery);
        }

        public virtual IGenericQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector)
        {
            if (_orderedQuery == null) _orderedQuery = _query.OrderBy(keySelector);
            else _orderedQuery.OrderBy(keySelector);
            return this;
        }

        public virtual IGenericQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> keySelector)
        {
            if (_orderedQuery == null) _orderedQuery = _query.OrderByDescending(keySelector);
            else _orderedQuery.OrderByDescending(keySelector);
            return this;
        }

        public virtual async Task<int> CountAsync()
            => await _query.CountAsync();

        public virtual IGenericQuery<TEntity> Take(int take)
            => Set(q => q._take = take);

        public virtual async Task<IEnumerable<TEntity>> SelectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _query = _orderedQuery ?? _query;

            if (_skip.HasValue) _query = _query.Skip(_skip.Value);
            if (_take.HasValue) _query = _query.Take(_take.Value);

            return await _query.ToListAsync(cancellationToken);
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
            => await _query.FirstOrDefaultAsync(predicate);

        public virtual IGenericQuery<TEntity> ConditionalWhere(Func<bool> condition, Expression<Func<TEntity, bool>> predicate)
        {
            if (condition())
                return Where(predicate);

            return this;
        }

        public virtual async Task<bool> AnyAsync()
            => await _query.AnyAsync();

        public virtual async Task<object> MaxAsync(Expression<Func<TEntity, object>> keySelector)
        {
			var itemsExist = await _query.AnyAsync();
            if(itemsExist)
			{
			    var max = await _query.MaxAsync(keySelector);
                return max;
			}

            return 0;
        }

        private IGenericQuery<TEntity> Set(Action<GenericQuery<TEntity>> setParameter)
        {
            setParameter(this);
            return this;
        }
	}
}
