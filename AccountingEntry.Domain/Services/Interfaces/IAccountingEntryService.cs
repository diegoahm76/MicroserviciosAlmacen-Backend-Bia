using AccountingEntry.Domain.Model;
using AccountingEntry.Domain.Model.ModelQuery;
using System.Threading.Tasks;

namespace AccountingEntry.Domain.Services.Interfaces
{
	public interface IAccountingEntryService
	{
		Task<T85Documento> CreateOrUpdtaeAccountingSeat(RegistryInWareHouse registryInWareHouse, bool isCreate);
		Task<T85Documento> DeleteAccountingSeat(RegistryInWareHouse registryInWareHouse);
		Task<T85Documento> CanceledAccountingSeat(CanceledDocument canceledDocument);
	}
}
