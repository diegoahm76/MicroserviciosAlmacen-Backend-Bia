using AccountingEntry.Domain.Model;
using AccountingEntry.Domain.Model.ModelQuery;
using System.Threading.Tasks;

namespace AccountingEntry.Domain.Services.Interfaces
{
	public interface IAccountingEntryService
	{
		Task<T85Documento> AccountingSeat(RegistryInWareHouse registryInWareHouse);
	}
}
