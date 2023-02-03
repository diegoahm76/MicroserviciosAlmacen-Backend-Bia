using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountingEntry.Repository.Interfaces
{
	public interface IGenericUnitOfWork
	{
		Task<int> SaveChangesAsync();
		Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters);
	}
}
