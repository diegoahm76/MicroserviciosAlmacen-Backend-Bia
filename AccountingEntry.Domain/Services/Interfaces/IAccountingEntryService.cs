using AccountingEntry.Domain.Model.ModelQuery;
using System.Threading.Tasks;

namespace AccountingEntry.Domain.Services.Interfaces
{
	public interface IAccountingEntryService
	{
		Task<string> AccountingSeat(RegistryInWareHouse registryInWareHouse);
	}
}
