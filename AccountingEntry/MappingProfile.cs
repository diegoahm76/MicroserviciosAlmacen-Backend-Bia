using AccountingEntry.API.BindingModel;
using AccountingEntry.Domain.Model;
using AccountingEntry.Domain.Model.ModelQuery;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingEntry.API
{
	public class MappingProfile: Profile
	{
		public MappingProfile()
		{
			CreateMap<RegistryInWareHouseRequest, RegistryInWareHouse>().ReverseMap();
			CreateMap<AccountRequest, Account>().ReverseMap();
			CreateMap<T85Documento, DocumentTransaction>().ReverseMap();
			CreateMap<DeleteDocumentRequest, RegistryInWareHouse>().ReverseMap();
			CreateMap<CanceledDocumentRequest, CanceledDocument>().ReverseMap();
		}
	}
}
