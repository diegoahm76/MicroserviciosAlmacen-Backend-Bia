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
		/// Registra un Documento
		/// </summary>
		/// <param name="registryInWareHouseRequest"></param>
		/// <returns></returns>
		[Route("CreateAccountingSeat")]
		[HttpPost]
		public async Task<IActionResult> CreateAccountingSeat(RegistryInWareHouseRequest registryInWareHouseRequest)
		{
			try
			{
				var registryInWareHouse = _mapper.Map<RegistryInWareHouse>(registryInWareHouseRequest);
				var document = await _accountingEntryService.CreateOrUpdtaeAccountingSeat(registryInWareHouse, true);
				var documentResponse = _mapper.Map<DocumentTransaction>(document);
				return Ok(documentResponse);
			}
			catch (Exception e)
			{
				if (e is ApplicationException)
					return BadRequest(e.Message);
				else
					return BadRequest(e);
			}
		}

		/// <summary>
		/// Edita un Documento
		/// </summary>
		/// <param name="registryInWareHouseRequest"></param>
		/// <returns></returns>
		[Route("UpdateAccountingSeat")]
		[HttpPost]
		public async Task<IActionResult> UpdateAccountingSeat(RegistryInWareHouseRequest registryInWareHouseRequest)
		{
			try
			{
				var registryInWareHouse = _mapper.Map<RegistryInWareHouse>(registryInWareHouseRequest);
				var document = await _accountingEntryService.CreateOrUpdtaeAccountingSeat(registryInWareHouse, false);
				var documentResponse = _mapper.Map<DocumentTransaction>(document);
				return Ok(documentResponse);
			}
			catch (Exception e)
			{
				if (e is ApplicationException)
					return BadRequest(e.Message);
				else
					return BadRequest(e);
			}
		}

		/// <summary>
		/// Elimina un Documento
		/// </summary>
		/// <param name="deleteDocumentRequestRequest"></param>
		/// <returns></returns>
		[Route("DeleteAccountingSeat")]
		[HttpPost]
		public async Task<IActionResult> DeleteAccountingSeat(DeleteDocumentRequest deleteDocumentRequestRequest)
		{
			try
			{
				var registryInWareHouse = _mapper.Map<RegistryInWareHouse>(deleteDocumentRequestRequest);
				var document = await _accountingEntryService.DeleteAccountingSeat(registryInWareHouse);
				var documentResponse = _mapper.Map<DocumentTransaction>(document);
				return Ok(documentResponse);
			}
			catch (Exception e)
			{
				if (e is ApplicationException)
					return BadRequest(e.Message);
				else
					return BadRequest(e);
			}
		}

		/// <summary>
		/// Anula un Documento
		/// </summary>
		/// <param name="canceledDocumentRequest"></param>
		/// <returns></returns>
		[Route("CanceledAccountingSeat")]
		[HttpPost]
		public async Task<IActionResult> CanceledAccountingSeat(CanceledDocumentRequest canceledDocumentRequest)
		{
			try
			{
				var registryInWareHouse = _mapper.Map<CanceledDocument>(canceledDocumentRequest);
				var document = await _accountingEntryService.CanceledAccountingSeat(registryInWareHouse);
				var documentResponse = _mapper.Map<DocumentTransaction>(document);
				return Ok(documentResponse);
			}
			catch (Exception e)
			{
				if (e is ApplicationException)
					return BadRequest(e.Message);
				else
					return BadRequest(e);
			}
		}
	}
}
