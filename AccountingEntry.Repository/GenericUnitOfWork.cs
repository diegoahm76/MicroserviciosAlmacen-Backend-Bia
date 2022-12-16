using AccountingEntry.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountingEntry.Repository
{
	public class GenericUnitOfWork: IGenericUnitOfWork
	{
        protected DbContext Context { get; }

        public GenericUnitOfWork(DbContext context)
        {
            Context = context;
        }

        public virtual async Task<int> SaveChangesAsync()
            => await Context.SaveChangesAsync();

		public virtual async Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters)
			=> await Context.Database.ExecuteSqlRawAsync(sql, parameters);
	}
}
