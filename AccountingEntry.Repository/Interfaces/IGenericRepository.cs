using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingEntry.Repository.Interfaces
{
	public interface IGenericRepository<TEntity> where TEntity : class
	{
        Task<TEntity> FindAsync(object[] keyValues);
        Task<TEntity> FindAsync<TKey>(TKey keyValue);
        Task<bool> ExistsAsync(object[] keyValues);
        Task<bool> ExistsAsync<TKey>(TKey keyValue);
        void Detach(TEntity item);
        Task<TEntity> Add(TEntity item);
        void Insert(TEntity item);
        void Update(TEntity item);
        Task<bool> UpdateAsync(TEntity item);
        void Delete(TEntity item);
        Task<bool> DeleteAsync(object[] keyValues);
        Task<bool> DeleteAsync<TKey>(TKey keyValue);
        IQueryable<TEntity> Queryable();
        IGenericQuery<TEntity> Query();
    }
}
