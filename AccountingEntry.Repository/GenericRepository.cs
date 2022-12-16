using AccountingEntry.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingEntry.Repository
{
	public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
	{
        private readonly IGenericQuery<TEntity> _query;
        protected DbContext Context { get; }
        protected DbSet<TEntity> Set { get; }

        public GenericRepository(DbContext context)
        {
            Context = context;
            Set = context.Set<TEntity>();
            _query = new GenericQuery<TEntity>(this);
            Context.Database.SetCommandTimeout(180);
        }

        public virtual async Task<TEntity> FindAsync(object[] keyValues)
            => await Set.FindAsync(keyValues);

        public virtual async Task<TEntity> FindAsync<TKey>(TKey keyValue)
            => await FindAsync(new object[] { keyValue });

        public virtual async Task<bool> ExistsAsync(object[] keyValues)
        {
            var item = await FindAsync(keyValues);
            return item != null;
        }

        public virtual async Task<bool> ExistsAsync<TKey>(TKey keyValue)
            => await ExistsAsync(new object[] { keyValue });

        public virtual void Detach(TEntity item)
            => Context.Entry(item).State = EntityState.Detached;
        public virtual void Insert(TEntity item)
            => Context.Entry(item).State = EntityState.Added;
        public virtual async Task<TEntity> Add(TEntity item)
        {
            Context.Entry(item).State = EntityState.Added;
            return item;
        }
        public virtual void Update(TEntity item)
            => Context.Entry(item).State = EntityState.Modified;

        public virtual async Task<bool> UpdateAsync(TEntity item)
        {
            var keyValue = GetKey(item);
            var currentItem = await FindAsync(new object[] { keyValue });
            if (currentItem == null) return false;
            Context.Entry(currentItem).CurrentValues.SetValues(item);
            Context.Entry(currentItem).State = EntityState.Modified;
            return true;
        }

        public virtual void Delete(TEntity item)
            => Context.Entry(item).State = EntityState.Deleted;

        public virtual async Task<bool> DeleteAsync(object[] keyValues)
        {
            var item = await FindAsync(keyValues);
            if (item == null) return false;
            Context.Entry(item).State = EntityState.Deleted;
            return true;
        }

        public virtual async Task<bool> DeleteAsync<TKey>(TKey keyValue)
            => await DeleteAsync(new object[] { keyValue });

        public virtual IQueryable<TEntity> Queryable() => Set;

        public virtual IGenericQuery<TEntity> Query()
        {
            _query.Clean();
            return _query;
        }

        public virtual object GetKey(TEntity entity)
        {
            var keyName = Context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();

            return entity.GetType().GetProperty(keyName).GetValue(entity, null);
        }
    }
}
