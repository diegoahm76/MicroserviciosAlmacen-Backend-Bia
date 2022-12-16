using AccountingEntry.API.BindingModel;
using AccountingEntry.Domain.Model.ModelQuery;
using AccountingEntry.Domain.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AccountingEntry.API.Controllers
{
	/// <summary>
	/// AccountingEntry Controller
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class AccountingEntryController : Controller
	{
		private readonly IAccountingEntryService _accountingEntryService;
		private readonly IMapper _mapper;
		public AccountingEntryController(IAccountingEntryService accountingEntryService, IMapper mapper)
		{
			_accountingEntryService = accountingEntryService ?? throw new ArgumentException(nameof(accountingEntryService));
			_mapper = mapper;
		}

		/// <summary>
		/// Registra un asiento contable
		/// </summary>
		/// <returns></returns>
		[Route("AccountingSeat")]
		[HttpPost]
		public async Task<string> AccountingSeat(RegistryInWareHouseRequest registryInWareHouseRequest)
		{
			var registryInWareHouse = _mapper.Map<RegistryInWareHouse>(registryInWareHouseRequest);
			return await _accountingEntryService.AccountingSeat(registryInWareHouse);
		}
	}
}
