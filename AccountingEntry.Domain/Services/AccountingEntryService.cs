using AccountingEntry.Domain.Model;
using AccountingEntry.Domain.Model.ModelQuery;
using AccountingEntry.Domain.Services.Interfaces;
using AccountingEntry.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace AccountingEntry.Domain.Services
{
	public class AccountingEntryService : IAccountingEntryService
	{
		private readonly IGenericRepository<Registry> _registryRepository;
		private readonly IGenericRepository<Sypsysaudit> _sypsysauditRepository;
		private readonly IGenericRepository<SypsysUsers> _sypsysUsersRepository;
		private readonly IGenericRepository<SysApplication> _sysApplicationRepository;
		private readonly IGenericRepository<T01Cia> _t01CiaRepository;
		private readonly IGenericRepository<T03Tercero> _t03TerceroRepository;
		private readonly IGenericRepository<T80Cuenta> _t80CuentaRepository;
		private readonly IGenericRepository<T84CentroCosto> _t84CentroCostoRepository;
		private readonly IGenericRepository<T85Documento> _t85Documento;
		private readonly IGenericRepository<T86TipoDocumentoCia> _t86TipoDocumentoCiaRepository;
		private readonly IGenericRepository<T87Movimiento> _t87Movimiento;
		private readonly IGenericRepository<T88TotalCuenta> _t88TotalCuenta;
		private readonly IGenericRepository<T89TotalCentroCosto> _t89TotalCentroCosto;
		private readonly IGenericRepository<T90TotalTercero> _t90TotalTercero;
		private readonly IGenericUnitOfWork _unitOfWork;
		private List<T80Cuenta> accounts;

		public AccountingEntryService(IGenericRepository<Registry> registryRepository,
			IGenericRepository<Sypsysaudit> sypsysauditRepository,
			IGenericRepository<SypsysUsers> sypsysUsersRepository,
			IGenericRepository<SysApplication> sysApplicationRepository,
			IGenericRepository<T01Cia> t01CiaRepository,
			IGenericRepository<T03Tercero> t03TerceroRepository,
			IGenericRepository<T80Cuenta> t80CuentaRepository,
			IGenericRepository<T84CentroCosto> t84CentroCostoRepository,
			IGenericRepository<T85Documento> t85Documento,
			IGenericRepository<T86TipoDocumentoCia> t86TipoDocumentoCiaRepository,
			IGenericRepository<T87Movimiento> t87Movimiento,
			IGenericRepository<T88TotalCuenta> t88TotalCuenta,
			IGenericRepository<T89TotalCentroCosto> t89TotalCentroCosto,
			IGenericRepository<T90TotalTercero> t90TotalTercero,
			IGenericUnitOfWork unitOfWork)
		{
			_registryRepository = registryRepository;
			_sypsysauditRepository = sypsysauditRepository;
			_sypsysUsersRepository = sypsysUsersRepository;
			_sysApplicationRepository = sysApplicationRepository;
			_t01CiaRepository = t01CiaRepository;
			_t03TerceroRepository = t03TerceroRepository;
			_t80CuentaRepository = t80CuentaRepository;
			_t84CentroCostoRepository = t84CentroCostoRepository;
			_t85Documento = t85Documento;
			_t86TipoDocumentoCiaRepository = t86TipoDocumentoCiaRepository;
			_t87Movimiento = t87Movimiento;
			_t88TotalCuenta = t88TotalCuenta;
			_t89TotalCentroCosto = t89TotalCentroCosto;
			_t90TotalTercero = t90TotalTercero;
			_unitOfWork = unitOfWork;
			accounts = new List<T80Cuenta>();
		}
		public async Task<T85Documento> CreateOrUpdtaeAccountingSeat(RegistryInWareHouse registryInWareHouse, bool isCreate)
		{
			string messageTransaction = await ValidationsBeforeSaveDocument(registryInWareHouse, isCreate, true);
			if(messageTransaction == "OK")
			{
				T85Documento createOrUpdateDocument = isCreate ? await CreateDocumentAndMovement(registryInWareHouse) : await UpdateDocumentAndMovement(registryInWareHouse);
				return createOrUpdateDocument;
			}else
			{
				throw new ApplicationException(messageTransaction);
			}
		}

		public async Task<T85Documento> DeleteAccountingSeat(RegistryInWareHouse registryInWareHouse)
		{
			registryInWareHouse.FechaAnterior = registryInWareHouse.FechaNueva;
			string messageTransaction = await ValidationsBeforeSaveDocument(registryInWareHouse, false, false);
			if (messageTransaction == "OK")
			{
				T85Documento documentDB = await GetDocument(registryInWareHouse.CodCia, registryInWareHouse.FechaNueva, registryInWareHouse.CodTipoDoc, registryInWareHouse.NumeroDoc);
				if (documentDB != null)
				{
                    TransactionOptions options = new TransactionOptions();
                    options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                    options.Timeout = TransactionManager.MaximumTimeout;
                    using (var transaction = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled))
					{
                        await DeleteMovementsAndTotals(registryInWareHouse);
                        await DeleteDocument(registryInWareHouse);
                        await SaveAuditRegistries(documentDB, registryInWareHouse.IdUsr, "T85DOCUMENTO", registryInWareHouse.ComputerAud, 2);
						transaction.Complete();
						return documentDB;
                    }	
				}
				else
				{
					throw new ApplicationException(MessagesError(15, ""));
				}
			}
			else
			{
				throw new ApplicationException(messageTransaction);
			}
		}

		public async Task<T85Documento> CanceledAccountingSeat(CanceledDocument canceledDocument)
		{
			string messageTransaction = await ValidationsBeforeCanceledDocument(canceledDocument);
			if(messageTransaction == "OK") {
                TransactionOptions options = new TransactionOptions();
                options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                options.Timeout = TransactionManager.MaximumTimeout;
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled))
				{
					int maxNumberDocAnul = await MaxDocumentNumber(canceledDocument.CodCia, canceledDocument.FechaAnul, canceledDocument.CodTipoDocAnul);
					T85Documento documentCanceled = await InsertDocumentCanceled(canceledDocument, maxNumberDocAnul);
					T85Documento documentDB = await GetDocument(canceledDocument.CodCia, canceledDocument.FechaDoc, canceledDocument.CodTipoDoc, canceledDocument.NumeroDoc);
					if(documentDB != null)
					{
						documentDB.T85Anulado = "S";
						documentDB.T85NumeroDocAnul = maxNumberDocAnul + 1;
						await UpdateT85Documento(documentDB);
						await SaveAuditRegistries(documentDB, canceledDocument.IdUsr, "T85DOCUMENTO", canceledDocument.ComputerAud, 1);
					}else
					{
						int maxNumberDoc = await MaxDocumentNumber(canceledDocument.CodCia, canceledDocument.FechaDoc, canceledDocument.CodTipoDoc);
						RegistryInWareHouse registryInWareHouse = new RegistryInWareHouse();
						registryInWareHouse.CodCia = canceledDocument.CodCia;
						registryInWareHouse.FechaNueva = canceledDocument.FechaDoc;
						registryInWareHouse.CodTipoDoc = canceledDocument.CodTipoDoc;
						registryInWareHouse.NumeroDoc = maxNumberDoc + 1;
						registryInWareHouse.Concepto = canceledDocument.Concepto;
						registryInWareHouse.Anulado = "S";
						registryInWareHouse.NumeroDocAnul = maxNumberDocAnul + 1;
						registryInWareHouse.AppName = canceledDocument.AppName;
						registryInWareHouse.IdUsr = canceledDocument.IdUsr;
						registryInWareHouse.ComputerAud = canceledDocument.ComputerAud;
						await CreateDocument(registryInWareHouse);
					}

					transaction.Complete();
					return documentCanceled;
				}
			}
			else
			{
				throw new ApplicationException(messageTransaction);
			}
		}

		private async Task<T85Documento> InsertDocumentCanceled(CanceledDocument canceledDocument, int maxNumberDocAnul)
		{
			var movementsDB = await GetMovements(canceledDocument.CodCia, canceledDocument.FechaDoc, canceledDocument.CodTipoDoc, canceledDocument.NumeroDoc);
			RegistryInWareHouse registryInWareHouse = new RegistryInWareHouse();
			registryInWareHouse.CodCia = canceledDocument.CodCia;
			registryInWareHouse.FechaNueva = canceledDocument.FechaAnul;
			registryInWareHouse.CodTipoDoc = canceledDocument.CodTipoDocAnul;
			registryInWareHouse.NumeroDoc = maxNumberDocAnul + 1;
			registryInWareHouse.Concepto = canceledDocument.ConceptoAnul;
			registryInWareHouse.AppName = canceledDocument.AppName;
			registryInWareHouse.IdUsr = canceledDocument.IdUsr;
			registryInWareHouse.ComputerAud = canceledDocument.ComputerAud;

			if (movementsDB.Count() > 0)
			{
				if (registryInWareHouse.Cuentas == null) registryInWareHouse.Cuentas = new List<Account>();
				foreach (var movement in movementsDB)
				{
					Account cuenta = new Account()
					{
						CodCta = movement.T87CodCta,
						Detalle = movement.T87Detalle,
						ValorBase = movement.T87ValorBase,
						ValorDebito = movement.T87ValorCredito,
						ValorCredito = movement.T87ValorDebito
					};
					registryInWareHouse.Cuentas.Add(cuenta);
					registryInWareHouse.CodCentro = movement.T87CodCentro;
					registryInWareHouse.Nit = movement.T87Nit;
				}
				List<string> codCtas = movementsDB.Select(m => m.T87CodCta).ToList();
				accounts = (List<T80Cuenta>)await GetAccounts(registryInWareHouse.CodCia, registryInWareHouse.FechaNueva, codCtas);
				await SaveMovements(registryInWareHouse);
			}
			T85Documento documentCanceled = await CreateDocument(registryInWareHouse);
			return documentCanceled;
		}

		private async Task<T85Documento> CreateDocumentAndMovement(RegistryInWareHouse registryInWareHouse)
		{
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
			options.Timeout = TransactionManager.MaximumTimeout;
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled))
			{
				T85Documento document = await CreateDocument(registryInWareHouse);
				await SaveMovements(registryInWareHouse);
				transaction.Complete();
				return document;
			}
        }

		private async Task<T85Documento> CreateDocument(RegistryInWareHouse registryInWareHouse)
		{
			T85Documento document = SetModelDocument(registryInWareHouse);
			await _t85Documento.Add(document);
			await _unitOfWork.SaveChangesAsync();
			await SaveAuditRegistries(document, registryInWareHouse.IdUsr, "T85DOCUMENTO", registryInWareHouse.ComputerAud, 0);
			return document;
		}

		private async Task<T85Documento> UpdateDocumentAndMovement(RegistryInWareHouse registryInWareHouse)
		{
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            options.Timeout = TransactionManager.MaximumTimeout;
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled))
			{
				T85Documento documentUpdate = SetModelDocument(registryInWareHouse);
				await UpdateT85Documento(documentUpdate);
				await SaveAuditRegistries(documentUpdate, registryInWareHouse.IdUsr, "T85DOCUMENTO", registryInWareHouse.ComputerAud, 1);

				await DeleteMovementsAndTotals(registryInWareHouse);
				await SaveMovements(registryInWareHouse);
				transaction.Complete();
				return documentUpdate;
			}
		}

		private async Task<bool> SaveAuditRegistries(T85Documento document, int IdUsr, string Table, string ComputerAud, short TransAud)
		{
			Sypsysaudit audit = SetModelAudit(document, IdUsr, Table, ComputerAud, TransAud);
			await _sypsysauditRepository.Add(audit);
			await _unitOfWork.SaveChangesAsync();
			_sypsysauditRepository.Detach(audit);
			return true;
		}

		private async Task<bool> SaveMovements(RegistryInWareHouse registryInWareHouse)
		{
			List<SetTotals> movementsByAccount = new List<SetTotals>();
			registryInWareHouse.Cuentas = SimplyAccounts(registryInWareHouse.Cuentas);
			foreach (var cuenta in registryInWareHouse.Cuentas)
			{
				var maxNumeroDoc = (int)await _t87Movimiento.Query()
					.Where(m => m.T87CodCia.Equals(registryInWareHouse.CodCia) && m.T87Agno == (short)registryInWareHouse.FechaNueva.Date.Year &&
						m.T87CodTipoDoc.Equals(registryInWareHouse.CodTipoDoc) && m.T87NumeroDoc == registryInWareHouse.NumeroDoc && m.T87CodCta.Equals(cuenta.CodCta))
					.MaxAsync(m => m.T87ConsecCta);

				T87Movimiento movement = await AddMovement(registryInWareHouse, registryInWareHouse.NumeroDoc, maxNumeroDoc, cuenta);

				var indexMovement = movementsByAccount.FindIndex(mba => mba.CodCta.Equals(cuenta.CodCta));
				if(indexMovement != -1)
				{
					movementsByAccount[indexMovement].Movement.T87ValorDebito += movement.T87ValorDebito;
					movementsByAccount[indexMovement].Movement.T87ValorCredito += movement.T87ValorCredito;
				}else
				{
					movementsByAccount.Add(new SetTotals { Movement = movement, CodCta = cuenta.CodCta});
				}
			}

			await SaveTotals(registryInWareHouse, movementsByAccount, false);

			return true;
		}

        private List<Account> SimplyAccounts(List<Account> accounts)
		{
			List<Account> commonAccounts = new List<Account>();
			foreach(var account in accounts)
			{
				int indexAccount = commonAccounts.FindIndex(itemAccount => itemAccount.CodCta.Equals(account.CodCta) && itemAccount.Detalle.Equals(account.Detalle));
				if(indexAccount != -1)
				{
					if (commonAccounts[indexAccount].ValorDebito != 0 && account.ValorDebito != 0) commonAccounts[indexAccount].ValorDebito += account.ValorDebito;
					if(commonAccounts[indexAccount].ValorCredito != 0 && account.ValorCredito != 0) commonAccounts[indexAccount].ValorCredito += account.ValorCredito;

					if(commonAccounts[indexAccount].ValorDebito != 0 && commonAccounts[indexAccount].ValorCredito == 0 && account.ValorDebito == 0 && account.ValorCredito != 0)
					{
						decimal ValorDebito = commonAccounts[indexAccount].ValorDebito >= account.ValorCredito ? commonAccounts[indexAccount].ValorDebito - account.ValorCredito : 0;
						decimal ValorCredito = commonAccounts[indexAccount].ValorDebito <= account.ValorCredito ? account.ValorCredito - commonAccounts[indexAccount].ValorDebito : 0;
						commonAccounts[indexAccount].ValorDebito = ValorDebito;
						commonAccounts[indexAccount].ValorCredito = ValorCredito;

                    }
                    else if (commonAccounts[indexAccount].ValorDebito == 0 && commonAccounts[indexAccount].ValorCredito != 0 && account.ValorDebito != 0 && account.ValorCredito == 0)
					{
                        decimal ValorDebito = commonAccounts[indexAccount].ValorCredito <= account.ValorDebito ? account.ValorDebito - commonAccounts[indexAccount].ValorCredito : 0;
                        decimal ValorCredito = commonAccounts[indexAccount].ValorCredito >= account.ValorDebito ? commonAccounts[indexAccount].ValorCredito - account.ValorDebito : 0;
                        commonAccounts[indexAccount].ValorDebito = ValorDebito;
                        commonAccounts[indexAccount].ValorCredito = ValorCredito;
                    }

					if (commonAccounts[indexAccount].ValorDebito == 0 && commonAccounts[indexAccount].ValorCredito == 0) commonAccounts.RemoveAt(indexAccount);
                }
                else
				{
					commonAccounts.Add(account);
				}
			}

			return commonAccounts;
		}

        private async Task<T87Movimiento> AddMovement(RegistryInWareHouse registryInWareHouse, int documentNumber, int maxNumeroDoc, Account cuenta)
		{
			T87Movimiento movement = SetModelMovement(registryInWareHouse, documentNumber, maxNumeroDoc, cuenta);
			await _t87Movimiento.Add(movement);
			await _unitOfWork.SaveChangesAsync();
			_t87Movimiento.Detach(movement);

			return movement;
		}

		private async Task<bool> UpdateT85Documento(T85Documento documentUpdate)
		{
			List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
			{
				new SqlParameter("@CodCia", documentUpdate.T85CodCia),
				new SqlParameter("@Agno", documentUpdate.T85Agno),
				new SqlParameter("@CodTipoDoc", documentUpdate.T85CodTipoDoc),
				new SqlParameter("@NumeroDoc", documentUpdate.T85NumeroDoc),
				new SqlParameter("@Fecha", documentUpdate.T85Fecha),
				new SqlParameter("@Concepto", documentUpdate.T85Concepto),
				new SqlParameter("@Anulado", documentUpdate.T85Anulado),
				new SqlParameter("@NumeroDocAnul", documentUpdate.T85NumeroDocAnul)
			};
			var updateString = "UPDATE [PIMISYS].[dbo].[T85DOCUMENTO] SET T85Agno = @Agno, T85Fecha = @Fecha, T85Concepto = @Concepto, T85Anulado = @Anulado, T85NumeroDocAnul = @NumeroDocAnul WHERE T85CodCia = @CodCia AND T85Agno = @Agno AND T85CodTipoDoc = @CodTipoDoc AND T85NumeroDoc = @NumeroDoc";
			await _unitOfWork.ExecuteSqlRawAsync(updateString, sqlParametersExtend);
			return true;
		}

		private async Task DeleteDocument(RegistryInWareHouse registryInWareHouse)
		{
			List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
			{
				new SqlParameter("@CodCia", registryInWareHouse.CodCia),
				new SqlParameter("@Agno", registryInWareHouse.FechaNueva.Date.Year),
				new SqlParameter("@CodTipoDoc", registryInWareHouse.CodTipoDoc),
				new SqlParameter("@NumeroDoc", registryInWareHouse.NumeroDoc)
			};
			var updateString = "DELETE FROM [PIMISYS].[dbo].[T85DOCUMENTO] WHERE T85CodCia = @CodCia AND T85Agno = @Agno AND T85CodTipoDoc = @CodTipoDoc AND T85NumeroDoc = @NumeroDoc";
			await _unitOfWork.ExecuteSqlRawAsync(updateString, sqlParametersExtend);
		}

		private async Task DeleteMovementsAndTotals(RegistryInWareHouse registryInWareHouse)
		{
			var movementsInDb = await GetMovements(registryInWareHouse.CodCia, registryInWareHouse.FechaNueva, registryInWareHouse.CodTipoDoc, registryInWareHouse.NumeroDoc);
			if (movementsInDb.Count() > 0)
			{
				List<string> codCtas = movementsInDb.Select(m => m.T87CodCta).ToList();
				var accountsForMovements = await GetAccounts(registryInWareHouse.CodCia, registryInWareHouse.FechaNueva, codCtas);
				var listCounts = accountsForMovements.Concat(accounts);
				accounts = listCounts.GroupBy(car => car.T80CodCta).Select(g => g.First()).ToList();
				await DeleteMovements(registryInWareHouse.CodCia, registryInWareHouse.FechaNueva.Date.Year, registryInWareHouse.CodTipoDoc, registryInWareHouse.NumeroDoc);

				List<SetTotals> movementsByAccount = movementsInDb.Select(m => new SetTotals { Movement = m, CodCta = m.T87CodCta }).ToList();
				await SaveTotals(registryInWareHouse, movementsByAccount, true);
			}
		}

		private async Task<bool> DeleteMovements(string codCia, int agno, string codTipoDoc, int numeroDoc)
		{
			List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
			{
				new SqlParameter("@CodCia", codCia),
				new SqlParameter("@Agno", agno),
				new SqlParameter("@CodTipoDoc", codTipoDoc),
				new SqlParameter("@NumeroDoc", numeroDoc)
			};
			var updateString = "DELETE FROM [PIMISYS].[dbo].[T87MOVIMIENTO] WHERE T87CodCia = @CodCia AND T87Agno = @Agno AND T87CodTipoDoc = @CodTipoDoc AND T87NumeroDoc = @NumeroDoc";
			await _unitOfWork.ExecuteSqlRawAsync(updateString, sqlParametersExtend);
			return true;
		}

		private async Task<bool> SaveTotals(RegistryInWareHouse registryInWareHouse, List<SetTotals> movementsByAccount, bool isSubtract)
		{
			foreach(var movementByAccount in movementsByAccount)
			{
				DateTime fecha = isSubtract ? registryInWareHouse.FechaAnterior : registryInWareHouse.FechaNueva;
				await SaveTotalAccount(registryInWareHouse, movementByAccount.Movement, movementByAccount.CodCta, isSubtract, fecha);

				T80Cuenta account = accounts.Find(acc => acc.T80CodCta == movementByAccount.CodCta);
				if (account.T80CentroCosto.Equals("S"))
				{
					await SaveTotalCostCenter(registryInWareHouse, movementByAccount.Movement, movementByAccount.CodCta, isSubtract, fecha);
				}

				if (account.T80Tercero.Equals("S"))
				{
					await SaveTotalPerson(registryInWareHouse, movementByAccount.Movement, movementByAccount.CodCta, isSubtract, fecha);
				}
			}
			
			return true;
		}

		private async Task<bool> SaveTotalAccount(RegistryInWareHouse registryInWareHouse, T87Movimiento movement, string codCta,bool isSubtract, DateTime fecha)
		{
			T88TotalCuenta totalCountBefore = await _t88TotalCuenta.Query()
					.FirstOrDefaultAsync(t => t.T88CodCia.Equals(registryInWareHouse.CodCia) && t.T88Agno == (short)fecha.Date.Year &&
						t.T88CodCta.Equals(codCta) && t.T88Mes == fecha.Date.Month);
			T88TotalCuenta totalAccount = SetModelTotalAccount(movement, totalCountBefore, isSubtract);
			if (totalCountBefore != null)
			{
				await UpdateT88TotalCuenta(totalAccount);
			}
			else
			{
				List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
				{
					new SqlParameter("@CodCia", totalAccount.T88CodCia),
					new SqlParameter("@Agno", totalAccount.T88Agno),
					new SqlParameter("@CodCta", totalAccount.T88CodCta)
				};
				var insertString = "INSERT INTO T88TOTALCUENTA(T88CodCia,T88Agno,T88CodCta,T88Mes,T88MovDebitoLocal,T88MovCreditoLocal,T88MovDebitoNIIF,T88MovCreditoNIIF)" +
					"SELECT @CodCia, @Agno, @CodCta, S08Mes,0.00,0.00,0.00,0.00 FROM S08MES WHERE S08TotalesPYG = 'S'";
				await _unitOfWork.ExecuteSqlRawAsync(insertString, sqlParametersExtend);
				await UpdateT88TotalCuenta(totalAccount);
			}

			_t88TotalCuenta.Detach(totalAccount);

			return true;
		}

		private async Task<bool> SaveTotalCostCenter(RegistryInWareHouse registryInWareHouse, T87Movimiento movement, string codCta, bool isSubtract, DateTime fecha)
		{
			string CodCentro = isSubtract ? movement.T87CodCentro : registryInWareHouse.CodCentro;
			T89TotalCentroCosto totalCostCenterBefore = await _t89TotalCentroCosto.Query()
				.FirstOrDefaultAsync(tc => tc.T89CodCia.Equals(registryInWareHouse.CodCia) && tc.T89Agno == (short)fecha.Date.Year &&
					tc.T89CodCta.Equals(codCta) && tc.T89CodCentro.Equals(CodCentro) && tc.T89Mes == fecha.Date.Month);

			T89TotalCentroCosto totalCostCenter = SetModelTotalCostCenter(movement, totalCostCenterBefore, isSubtract);
			if (totalCostCenterBefore != null)
			{
				await UpdateT89TotalCentroCosto(totalCostCenter);
			}
			else
			{
				List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
				{
					new SqlParameter("@CodCia", totalCostCenter.T89CodCia),
					new SqlParameter("@Agno", totalCostCenter.T89Agno),
					new SqlParameter("@CodCta", totalCostCenter.T89CodCta),
					new SqlParameter("@CodCentro", totalCostCenter.T89CodCentro)
				};
				var insertString = "INSERT INTO T89TOTALCENTROCOSTO(T89CodCia,T89Agno,T89CodCta,T89CodCentro,T89Mes,T89MovDebitoLocal,T89MovCreditoLocal,T89MovDebitoNIIF,T89MovCreditoNIIF)" +
					"SELECT @CodCia, @Agno, @CodCta, @CodCentro,S08Mes,0.00,0.00,0.00,0.00 FROM S08MES WHERE S08TotalesPYG = 'S'";
				await _unitOfWork.ExecuteSqlRawAsync(insertString, sqlParametersExtend);
				await UpdateT89TotalCentroCosto(totalCostCenter);
			}

			_t89TotalCentroCosto.Detach(totalCostCenter);

			return true;
		}

		private async Task<bool> SaveTotalPerson(RegistryInWareHouse registryInWareHouse, T87Movimiento movement, string codCta, bool isSubtract, DateTime fecha)
		{
			string Nit = isSubtract ? movement.T87Nit : registryInWareHouse.Nit;
			T90TotalTercero totalPersonBefore = await _t90TotalTercero.Query()
				.FirstOrDefaultAsync(tn => tn.T90CodCia.Equals(registryInWareHouse.CodCia) && tn.T90Agno == (short)fecha.Date.Year &&
					tn.T90CodCta.Equals(codCta) && tn.T90Nit.Equals(Nit) && tn.T90Mes == fecha.Date.Month);

			T90TotalTercero totalPerson = SetModelTotalPerson(movement, totalPersonBefore, isSubtract);
			if (totalPersonBefore != null)
			{
				await UpdateT90TotalTercero(totalPerson);
			}
			else
			{
				List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
				{
					new SqlParameter("@CodCia", totalPerson.T90CodCia),
					new SqlParameter("@Agno", totalPerson.T90Agno),
					new SqlParameter("@CodCta", totalPerson.T90CodCta),
					new SqlParameter("@Nit", totalPerson.T90Nit)
				};
				var insertString = "INSERT INTO T90TOTALTERCERO(T90CodCia,T90Agno,T90CodCta,T90Nit,T90Mes,T90MovDebitoLocal,T90MovCreditoLocal,T90MovDebitoNIIF,T90MovCreditoNIIF)" +
					"SELECT @CodCia, @Agno, @CodCta, @Nit,S08Mes,0.00,0.00,0.00,0.00 FROM S08MES WHERE S08TotalesPYG = 'S'";
				await _unitOfWork.ExecuteSqlRawAsync(insertString, sqlParametersExtend);
				await UpdateT90TotalTercero(totalPerson);
			}

			_t90TotalTercero.Detach(totalPerson);

			return true;
		}

		private async Task UpdateT88TotalCuenta(T88TotalCuenta totalAccount)
		{
			List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
			{
				new SqlParameter("@T88MovDebitoLocal", totalAccount.T88MovDebitoLocal),
				new SqlParameter("@T88MovCreditoLocal", totalAccount.T88MovCreditoLocal),
				new SqlParameter("@T88MovDebitoNIIF", totalAccount.T88MovDebitoNIIF),
				new SqlParameter("@T88MovCreditoNIIF", totalAccount.T88MovCreditoNIIF),
				new SqlParameter("@CodCia", totalAccount.T88CodCia),
				new SqlParameter("@Agno", totalAccount.T88Agno),
				new SqlParameter("@CodCta", totalAccount.T88CodCta),
				new SqlParameter("@Mes", totalAccount.T88Mes)
			};
			var updateString = "UPDATE [PIMISYS].[dbo].[T88TOTALCUENTA] SET T88MovDebitoLocal = @T88MovDebitoLocal, T88MovCreditoLocal = @T88MovCreditoLocal, T88MovDebitoNIIF = @T88MovDebitoNIIF, T88MovCreditoNIIF = @T88MovCreditoNIIF WHERE T88CodCia = @CodCia AND T88Agno = @Agno AND T88CodCta = @CodCta AND T88Mes = @Mes";
			await _unitOfWork.ExecuteSqlRawAsync(updateString, sqlParametersExtend);
		}

		private async Task UpdateT89TotalCentroCosto(T89TotalCentroCosto totalCostCenter)
		{
			List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
			{
				new SqlParameter("@T89MovDebitoLocal", totalCostCenter.T89MovDebitoLocal),
				new SqlParameter("@T89MovCreditoLocal", totalCostCenter.T89MovCreditoLocal),
				new SqlParameter("@T89MovDebitoNIIF", totalCostCenter.T89MovDebitoNIIF),
				new SqlParameter("@T89MovCreditoNIIF", totalCostCenter.T89MovCreditoNIIF),
				new SqlParameter("@CodCia", totalCostCenter.T89CodCia),
				new SqlParameter("@Agno", totalCostCenter.T89Agno),
				new SqlParameter("@CodCta", totalCostCenter.T89CodCta),
				new SqlParameter("@CodCentro", totalCostCenter.T89CodCentro),
				new SqlParameter("@Mes", totalCostCenter.T89Mes)
			};
			var updateString = "UPDATE [PIMISYS].[dbo].[T89TOTALCENTROCOSTO] SET T89MovDebitoLocal = @T89MovDebitoLocal, T89MovCreditoLocal = @T89MovCreditoLocal, T89MovDebitoNIIF = @T89MovDebitoNIIF, T89MovCreditoNIIF = @T89MovCreditoNIIF WHERE T89CodCia = @CodCia AND T89Agno = @Agno AND T89CodCta = @CodCta AND T89CodCentro = @CodCentro AND T89Mes = @Mes";
			await _unitOfWork.ExecuteSqlRawAsync(updateString, sqlParametersExtend);
		}

		private async Task UpdateT90TotalTercero(T90TotalTercero totalPerson)
		{
			List<SqlParameter> sqlParametersExtend = new List<SqlParameter>
			{
				new SqlParameter("@T90MovDebitoLocal", totalPerson.T90MovDebitoLocal),
				new SqlParameter("@T90MovCreditoLocal", totalPerson.T90MovCreditoLocal),
				new SqlParameter("@T90MovDebitoNIIF", totalPerson.T90MovDebitoNIIF),
				new SqlParameter("@T90MovCreditoNIIF", totalPerson.T90MovCreditoNIIF),
				new SqlParameter("@CodCia", totalPerson.T90CodCia),
				new SqlParameter("@Agno", totalPerson.T90Agno),
				new SqlParameter("@CodCta", totalPerson.T90CodCta),
				new SqlParameter("@Nit", totalPerson.T90Nit),
				new SqlParameter("@Mes", totalPerson.T90Mes)
			};
			var updateString = "UPDATE [PIMISYS].[dbo].[T90TOTALTERCERO] SET T90MovDebitoLocal = @T90MovDebitoLocal, T90MovCreditoLocal = @T90MovCreditoLocal, T90MovDebitoNIIF = @T90MovDebitoNIIF, T90MovCreditoNIIF = @T90MovCreditoNIIF WHERE T90CodCia = @CodCia AND T90Agno = @Agno AND T90CodCta = @CodCta AND T90Nit = @Nit AND T90Mes = @Mes";
			await _unitOfWork.ExecuteSqlRawAsync(updateString, sqlParametersExtend);
		}

		/*Common Querys Zone*/
		private async Task<int> MaxDocumentNumber(string CodCia, DateTime Fecha, string CodTipoDocAnul)
		{
			int maxNumberDocAnul = (int)await _t85Documento.Query()
					.Where(d => d.T85CodCia.Equals(CodCia) && d.T85Agno == Fecha.Date.Year &&
						d.T85CodTipoDoc.Equals(CodTipoDocAnul))
					.MaxAsync(d => d.T85NumeroDoc);
			return maxNumberDocAnul;
		}

		private async Task<T85Documento> GetDocument(string CodCia, DateTime FechaDoc, string CodTipoDoc, int NumeroDoc)
		{
			T85Documento documentInDB = await _t85Documento.Query()
						.FirstOrDefaultAsync(d => d.T85CodCia.Equals(CodCia) && d.T85Agno == FechaDoc.Date.Year &&
							d.T85CodTipoDoc.Equals(CodTipoDoc) && d.T85NumeroDoc == NumeroDoc);
			return documentInDB;
		}

		private async Task<IEnumerable<T87Movimiento>> GetMovements(string CodCia, DateTime FechaDoc, string CodTipoDoc, int NumeroDoc)
		{
			var movementsInDb = await _t87Movimiento.Query()
					.Where(m => m.T87CodCia.Equals(CodCia) && m.T87Agno == (short)FechaDoc.Date.Year &&
						m.T87CodTipoDoc.Equals(CodTipoDoc) && m.T87NumeroDoc == NumeroDoc)
					.SelectAsync();
			return movementsInDb;
		}

		private async Task<IEnumerable<T80Cuenta>> GetAccounts(string CodCia, DateTime Fecha, List<string> codCtas)
		{
			var accounts = await _t80CuentaRepository.Query()
						.Where(c => c.T80CodCia.Equals(CodCia) && c.T80Agno == Fecha.Date.Year)
						.ConditionalWhere(() => codCtas.Any(), c => codCtas.Contains(c.T80CodCta))
						.SelectAsync();
			return accounts;
		}

		/*Validations zone*/
		private async Task<string> ValidationsBeforeSaveDocument(RegistryInWareHouse registryInWareHouse, bool isCreate, bool callCreateOrUpdate)
		{
			string messageTransaction = ValidateFieldsRequired(registryInWareHouse, isCreate, callCreateOrUpdate);
			if(messageTransaction == "OK")
			{
				messageTransaction = ValidateTotalsAccounts(registryInWareHouse, callCreateOrUpdate);
				if (messageTransaction == "OK")
				{ 
					messageTransaction = await ValidateCompany(registryInWareHouse.CodCia); // Preguntar si este dato es necesario validarlo 
					if (messageTransaction == "OK")
					{
						messageTransaction = await ValidateApplicationName(registryInWareHouse.AppName); // Preguntar si este dato es necesario validarlo
						if (messageTransaction == "OK")
						{
							messageTransaction = await ValidatePeriodLock(registryInWareHouse.CodCia, registryInWareHouse.FechaNueva.Date, registryInWareHouse.FechaAnterior.Date);
							if (messageTransaction == "OK")
							{
								messageTransaction = await ValidateDocumentTypeCode(registryInWareHouse.CodTipoDoc, registryInWareHouse.CodCia);
								if (messageTransaction == "OK")
								{
									messageTransaction = await ValidateUser(registryInWareHouse.IdUsr);
									if(messageTransaction == "OK" && callCreateOrUpdate)
									{
										messageTransaction = registryInWareHouse.Cuentas.Count > 1 ? await ValidateAccountByDateAndCompany(registryInWareHouse) : MessagesError(3, "");
									}
								}
							}
						}
					}
				}
			}

			return messageTransaction;
		}

		private async Task<string> ValidationsBeforeCanceledDocument(CanceledDocument canceledDocument)
		{
			string messageTransaction = ValidateFieldsRequiredToCanceled(canceledDocument);
			if (messageTransaction == "OK")
			{
				messageTransaction = await ValidateCompany(canceledDocument.CodCia);
				if(messageTransaction == "OK")
				{
					messageTransaction = await ValidateApplicationName(canceledDocument.AppName);
					if (messageTransaction == "OK")
					{
                        messageTransaction = await ValidatePeriodLock(canceledDocument.CodCia, canceledDocument.FechaAnul.Date, new DateTime().Date);
						if (messageTransaction == "OK")
						{
							for(int i = 0; i < 2; i++)
							{
								messageTransaction = await ValidateDocumentTypeCode(i == 0 ? canceledDocument.CodTipoDoc : canceledDocument.CodTipoDocAnul, canceledDocument.CodCia);
								if (messageTransaction != "OK") break;
							}

							if (messageTransaction == "OK")
							{
								messageTransaction = await ValidateUser(canceledDocument.IdUsr);
							}
						}
					}
				}
			}

			return messageTransaction;
		}

		private async Task<string> ValidateCompany(string CodCia)
		{
			T01Cia cia = await _t01CiaRepository.Query().FirstOrDefaultAsync(c => c.T01CodCia.Equals(CodCia));
			return cia != null ? MessagesError(0, "") : MessagesError(13, "");
		}

		private async Task<string> ValidateApplicationName(string appName)
		{
			SysApplication application = await _sysApplicationRepository.Query().FirstOrDefaultAsync(a => a.AppName.Equals(appName));
			return application != null ? MessagesError(0, "") : MessagesError(14, "");
		}
		private async Task<string> ValidatePeriodLock(string CodCia, DateTime FechaActual, DateTime FechaAnterior)
		{
			List<DateTime> listDates = new List<DateTime>
			{
				FechaActual
			};
			if (!FechaAnterior.Equals(Activator.CreateInstance(typeof(DateTime))) && FechaAnterior != FechaActual) listDates.Add(FechaAnterior);
			string message = "";
			for(int i = 0; i < listDates.Count; i++) {
				string mes = listDates[i].Month < 10 ? '0' + listDates[i].Month.ToString() : listDates[i].Month.ToString();
				string nameRegistryFilter = "COMPANIAS" + @"\" + CodCia + @"\" + listDates[i].Year + @"\" + "BLOQUEOS" + @"\" + mes + @"\";
				Registry registry = await _registryRepository.Query().FirstOrDefaultAsync(r => (r.AppName.Contains("P&G") || r.AppName.Equals("*")) && r.Nombre.Equals(nameRegistryFilter));
				if (registry != null) {
					message = registry.Valor.Equals("0") ? MessagesError(0, "") : MessagesError(1, "");
					if (message != "OK") break;
				}else
				{
					message = MessagesError(2, "");
					break;
				}

				_registryRepository.Detach(registry);
			}

			return message;
		}

		private async Task<string> ValidateAccountByDateAndCompany(RegistryInWareHouse registryInWareHouse)
		{
			string message = "OK";
			foreach (var cuenta in registryInWareHouse.Cuentas)
			{
				if(!accounts.Exists(ac => ac.T80CodCta.Equals(cuenta.CodCta)))
				{
					T80Cuenta account = await _t80CuentaRepository.Query().FirstOrDefaultAsync(c => c.T80CodCta.Equals(cuenta.CodCta) &&
					c.T80CodCia.Equals(registryInWareHouse.CodCia) && c.T80Agno == registryInWareHouse.FechaNueva.Date.Year);
					if (account != null)
					{
						if (account.T80Movimiento.Equals("S"))
						{
							if (account.T80CentroCosto.Equals("S"))
							{
								message = await ValidateCostCenter(registryInWareHouse.CodCia, registryInWareHouse.CodCentro);
							}

							if (account.T80Tercero.Equals("S") && message.Equals("OK"))
							{
								message = await ValidatePerson(registryInWareHouse.Nit);
							}
						}
						else
						{
							message = MessagesError(5, "");
						}
					}
					else
					{
						message = MessagesError(4, "");
					}

					if (!message.Equals("OK"))
						break;

					accounts.Add(account);
				}
			}
			
			return message;
		}

		private async Task<string> ValidateCostCenter(string CodCia, string CodCentro)
		{
			if(CodCentro != null)
			{
				T84CentroCosto costCenter = await _t84CentroCostoRepository.Query().FirstOrDefaultAsync(cc => cc.T84CodCentro.Equals(CodCentro) &&
					cc.T84CodCia.Equals(CodCia) && cc.T84Movimiento.Equals("S"));
				return costCenter != null ? MessagesError(0, "") : MessagesError(6, "");
			}

			return MessagesError(7, "");
			
		}

		private async Task<string> ValidatePerson(string nit)
		{
			if(nit != null)
			{
				T03Tercero person = await _t03TerceroRepository.Query().FirstOrDefaultAsync(t => t.T03Nit.Equals(nit));
				return person != null ? MessagesError(0, "") : MessagesError(8, "");
			}

			return MessagesError(9, "");
		}

		private async Task<string> ValidateDocumentTypeCode(string CodTipoDoc, string CodCia)
		{
			T86TipoDocumentoCia documentType = await _t86TipoDocumentoCiaRepository.Query().FirstOrDefaultAsync(dt => dt.T86CodTipoDoc.Equals(CodTipoDoc) && dt.T86CodCia.Equals(CodCia));
			return documentType  != null ? MessagesError(0, "") : MessagesError(10, "");
		}

		private async Task<string> ValidateUser(int IdUsr)
		{
			SypsysUsers user = await _sypsysUsersRepository.Query().FirstOrDefaultAsync(us => us.IdUsr.Equals(IdUsr));
			return user != null ? MessagesError(0, "") : MessagesError(11, "");
		}

		private string ValidateFieldsRequired(RegistryInWareHouse registryInWareHouse, bool isCreate, bool callCreateOrUpdate)
		{
			string fieldNames = "";
			fieldNames = registryInWareHouse.CodCia == null ? fieldNames + "CodCia" : fieldNames;
			fieldNames = registryInWareHouse.FechaNueva.Equals(Activator.CreateInstance(typeof(DateTime))) ?
				fieldNames + ", Fecha" : fieldNames;
			fieldNames = registryInWareHouse.CodTipoDoc == null ? fieldNames + ", CodTipoDoc" : fieldNames;
			fieldNames = registryInWareHouse.NumeroDoc == 0 ? fieldNames + ", NumeroDoc" : fieldNames;
			fieldNames = registryInWareHouse.AppName == null ? fieldNames + ", AppName" : fieldNames;
			fieldNames = registryInWareHouse.ComputerAud == null ? fieldNames + ", ComputerAud" : fieldNames;
			if(callCreateOrUpdate)
			{
				fieldNames = !isCreate && (registryInWareHouse.FechaAnterior.Equals(Activator.CreateInstance(typeof(DateTime)))) ?
					fieldNames + "FechaAnterior" : fieldNames;
				fieldNames = registryInWareHouse.Concepto == null ? fieldNames + ", Concepto" : fieldNames;
				fieldNames = registryInWareHouse.Cuentas == null ? fieldNames + ", Cuentas" : validateFieldsCountsRequired(registryInWareHouse.Cuentas, fieldNames);
			}

			return fieldNames == "" ? MessagesError(0, "") : MessagesError(12, fieldNames);
		}

		private string ValidateFieldsRequiredToCanceled(CanceledDocument canceledDocument)
		{
			string fieldNames = "";
			fieldNames = canceledDocument.CodCia == null ? fieldNames + "CodCia" : fieldNames;
			fieldNames = canceledDocument.FechaDoc.Equals(Activator.CreateInstance(typeof(DateTime))) ?
				fieldNames + ", FechaDoc" : fieldNames;
			fieldNames = canceledDocument.FechaAnul.Equals(Activator.CreateInstance(typeof(DateTime))) ?
				fieldNames + ", FechaAnul" : fieldNames;
			fieldNames = canceledDocument.CodTipoDoc == null ? fieldNames + ", CodTipoDoc" : fieldNames;
			fieldNames = canceledDocument.CodTipoDocAnul == null ? fieldNames + ", CodTipoDocAnul" : fieldNames;
			fieldNames = canceledDocument.NumeroDoc == 0 ? fieldNames + ", NumeroDoc" : fieldNames;
			fieldNames = canceledDocument.ConceptoAnul == null ? fieldNames + ", ConceptoAnul" : fieldNames;
			fieldNames = canceledDocument.AppName == null ? fieldNames + ", AppName" : fieldNames;
			fieldNames = canceledDocument.ComputerAud == null ? fieldNames + ", ComputerAud" : fieldNames;

			return fieldNames == "" ? MessagesError(0, "") : MessagesError(12, fieldNames);
		}

		private string validateFieldsCountsRequired(List<Account> cuentas, string fieldNames)
		{
			if(cuentas.Count > 0)
			{
				int loop = 0;
				foreach (Account cuenta in cuentas) {
					fieldNames = cuenta.CodCta == null ? fieldNames + ", CodCta en la posición " + loop + " de Cuentas" : fieldNames;
					fieldNames = cuenta.Detalle == null ? fieldNames + ", Detalle en la posición " + loop + " de Cuentas" : fieldNames;
					fieldNames = cuenta.ValorDebito == 0 && cuenta.ValorCredito == 0 ? fieldNames + ", ValorDebito o ValorCredito en la posición " + loop + " de Cuentas" : fieldNames;
					loop++;
				}
				return fieldNames;
			}else
			{
				return fieldNames + ", Cuentas";
			}
		}

		private string ValidateTotalsAccounts(RegistryInWareHouse registryInWareHouse, bool callCreateOrUpdate)
		{
			string message = "OK";
			if (callCreateOrUpdate)
			{
				decimal debito = 0;
				decimal credito = 0;
				foreach (Account cuenta in registryInWareHouse.Cuentas)
				{
					debito += cuenta.ValorDebito;
					credito += cuenta.ValorCredito;
				}
				message = debito == credito ? MessagesError(0, "") : MessagesError(16, "");
			}
			return message;
		}

		private T85Documento SetModelDocument(RegistryInWareHouse registryInWareHouse)
		{
			T85Documento document = new T85Documento();
			document.T85CodCia = registryInWareHouse.CodCia;
			document.T85Agno = (short)registryInWareHouse.FechaNueva.Date.Year;
			document.T85CodTipoDoc = registryInWareHouse.CodTipoDoc;
			document.T85NumeroDoc = registryInWareHouse.NumeroDoc;
			document.T85Fecha = registryInWareHouse.FechaNueva;
			document.T85Concepto = registryInWareHouse.Concepto;
			document.T85Anulado = registryInWareHouse.Anulado != null ? registryInWareHouse.Anulado : "N";
			document.T85AppName = registryInWareHouse.AppName;
			document.T85NumeroDocAnul = registryInWareHouse.NumeroDocAnul;
			document.T85CodTipoOrigenDoc = registryInWareHouse.CodTipoOrigenDoc != null ? registryInWareHouse.CodTipoOrigenDoc : "";
			document.T85NumeroOrigenDoc = registryInWareHouse.NumeroOrigenDoc;
			document.T85ReferenciaOrigenDoc = registryInWareHouse.ReferenciaOrigenDoc != null ? registryInWareHouse.ReferenciaOrigenDoc : "";
			document.T85ContabLocal = "N";
			document.T85ContabNIIF = "S";
			document.T85NumRevelacion = registryInWareHouse.NumRevelacion;
			return document;
		}

		private T87Movimiento SetModelMovement(RegistryInWareHouse registryInWareHouse, int documentNumber, int lastConsecCta, Account account)
		{
			T80Cuenta accountObject = accounts.FirstOrDefault(ac => ac.T80CodCta.Equals(account.CodCta));
			T87Movimiento movement = new T87Movimiento();
			movement.T87CodCia = registryInWareHouse.CodCia;
			movement.T87Agno = (short)registryInWareHouse.FechaNueva.Date.Year;
			movement.T87CodTipoDoc = registryInWareHouse.CodTipoDoc;
			movement.T87NumeroDoc = documentNumber;
			movement.T87CodCta = account.CodCta;
			movement.T87ConsecCta = lastConsecCta + 1;
			movement.T87Fecha = registryInWareHouse.FechaNueva;
			movement.T87CodCentro = accountObject.T80CentroCosto.Equals("S") ? registryInWareHouse.CodCentro : "";
			movement.T87Nit = accountObject.T80Tercero.Equals("S") ? registryInWareHouse.Nit : "";
			movement.T87Referencia = registryInWareHouse.ReferenciaMov != null ? registryInWareHouse.ReferenciaMov : "";
			movement.T87Detalle = account.Detalle != null ? account.Detalle : "";
			movement.T87CodTipoDocCruce = registryInWareHouse.CodTipoDocCruceMov != null ? registryInWareHouse.CodTipoDocCruceMov : "";
			movement.T87NumeroDocCruce = registryInWareHouse.NumeroDocCruceMov != null ? registryInWareHouse.NumeroDocCruceMov : "";
			movement.T87ValorBase = account.ValorBase;
			movement.T87ValorDebito = account.ValorDebito;
			movement.T87ValorCredito = account.ValorCredito;
			movement.T87ContabLocal = "N";
			movement.T87ContabNIIF = "S";
			movement.T87NumRevelacion = registryInWareHouse.NumRevelacion;

			return movement;
		}

		private T88TotalCuenta SetModelTotalAccount(T87Movimiento movement, T88TotalCuenta totalCountBefore, bool isSubtract)
		{
			T88TotalCuenta totalAccount = new T88TotalCuenta();
			totalAccount.T88CodCia = movement.T87CodCia;
			totalAccount.T88Agno = (short)movement.T87Fecha.Date.Year;
			totalAccount.T88CodCta = movement.T87CodCta;
			totalAccount.T88Mes = (byte)movement.T87Fecha.Date.Month;
			totalAccount.T88MovDebitoLocal = movement.T87ContabLocal.Equals("S") ? SetTotal(totalCountBefore != null ? totalCountBefore.T88MovDebitoLocal : 0, movement.T87ValorDebito, isSubtract) : 0;
			totalAccount.T88MovCreditoLocal = movement.T87ContabLocal.Equals("S") ? SetTotal(totalCountBefore != null ? totalCountBefore.T88MovCreditoLocal : 0, movement.T87ValorCredito, isSubtract) : 0;
			totalAccount.T88MovDebitoNIIF = movement.T87ContabNIIF.Equals("S") ? SetTotal(totalCountBefore != null ? totalCountBefore.T88MovDebitoNIIF : 0, movement.T87ValorDebito, isSubtract) : 0;
			totalAccount.T88MovCreditoNIIF = movement.T87ContabNIIF.Equals("S") ? SetTotal(totalCountBefore != null ? totalCountBefore.T88MovCreditoNIIF : 0, movement.T87ValorCredito, isSubtract) : 0;

			return totalAccount;
		}

		private T89TotalCentroCosto SetModelTotalCostCenter(T87Movimiento movement, T89TotalCentroCosto totalCostCenterBefore, bool isSubtract)
		{
			T89TotalCentroCosto totalCostCenter = new T89TotalCentroCosto();
			totalCostCenter.T89CodCia = movement.T87CodCia;
			totalCostCenter.T89Agno = (short)movement.T87Fecha.Date.Year;
			totalCostCenter.T89CodCta = movement.T87CodCta;
			totalCostCenter.T89CodCentro = movement.T87CodCentro;
			totalCostCenter.T89Mes = (byte)movement.T87Fecha.Date.Month;
			totalCostCenter.T89MovDebitoLocal = movement.T87ContabLocal.Equals("S") ? SetTotal(totalCostCenterBefore != null ? totalCostCenterBefore.T89MovDebitoLocal : 0, movement.T87ValorDebito, isSubtract) : 0;
			totalCostCenter.T89MovCreditoLocal = movement.T87ContabLocal.Equals("S") ? SetTotal(totalCostCenterBefore != null ? totalCostCenterBefore.T89MovCreditoLocal : 0, movement.T87ValorCredito, isSubtract) : 0;
			totalCostCenter.T89MovDebitoNIIF = movement.T87ContabNIIF.Equals("S") ? SetTotal(totalCostCenterBefore != null ? totalCostCenterBefore.T89MovDebitoNIIF : 0, movement.T87ValorDebito, isSubtract) : 0;
			totalCostCenter.T89MovCreditoNIIF = movement.T87ContabNIIF.Equals("S") ? SetTotal(totalCostCenterBefore != null ? totalCostCenterBefore.T89MovCreditoNIIF : 0, movement.T87ValorCredito, isSubtract) : 0;

			return totalCostCenter;
		}

		private T90TotalTercero SetModelTotalPerson(T87Movimiento movement, T90TotalTercero totalPersonBefore, bool isSubtract)
		{
			T90TotalTercero totalPerson = new T90TotalTercero();
			totalPerson.T90CodCia = movement.T87CodCia;
			totalPerson.T90Agno = (short)movement.T87Fecha.Date.Year;
			totalPerson.T90CodCta = movement.T87CodCta;
			totalPerson.T90Nit = movement.T87Nit;
			totalPerson.T90Mes = (byte)movement.T87Fecha.Date.Month;
			totalPerson.T90MovDebitoLocal = movement.T87ContabLocal.Equals("S") ? SetTotal(totalPersonBefore != null ? totalPersonBefore.T90MovDebitoLocal : 0, movement.T87ValorDebito, isSubtract) : 0;
			totalPerson.T90MovCreditoLocal = movement.T87ContabLocal.Equals("S") ? SetTotal(totalPersonBefore != null ? totalPersonBefore.T90MovCreditoLocal : 0, movement.T87ValorCredito, isSubtract) : 0;
			totalPerson.T90MovDebitoNIIF = movement.T87ContabNIIF.Equals("S") ? SetTotal(totalPersonBefore != null ? totalPersonBefore.T90MovDebitoNIIF : 0, movement.T87ValorDebito, isSubtract) : 0;
			totalPerson.T90MovCreditoNIIF = movement.T87ContabNIIF.Equals("S") ? SetTotal(totalPersonBefore != null ? totalPersonBefore.T90MovCreditoNIIF : 0, movement.T87ValorCredito, isSubtract) : 0;

			return totalPerson;
		}

		private decimal SetTotal(decimal valueExist, decimal valueNew, bool isSubtract)
		{ 
			return isSubtract ? valueExist - valueNew  : valueExist + valueNew; 
		}

		private Sypsysaudit SetModelAudit(T85Documento document, int idUsr, string TablaAud, string ComputerAud, short TransAud)
		{
			var concepto = document.T85Concepto.Count() > 50 ? document.T85Concepto.Remove(50) : document.T85Concepto;
			Sypsysaudit audit = new Sypsysaudit();
			audit.FechaAud = DateTime.Now;
			audit.IdUsr = idUsr;
			audit.TablaAud = TablaAud;
			audit.TransAud = TransAud;
			audit.DescAud = document.T85CodCia + ":" + document.T85Agno + ":" + document.T85CodTipoDoc + ":" + document.T85NumeroDoc + ":" + 
				document.T85Fecha.ToString("dd/MM/yyyy") + ":" + document.T85Anulado + ":" + document.T85AppName + ":" + concepto +
				":" + ":" + document.T85NumeroDocAnul;
			audit.ComputerAud = ComputerAud;

			return audit;
		}

		private string MessagesError(int codError, string fieldNames)
		{
			string error = "";
			switch(codError)
			{
				case 0:
					error = "OK";
					break;
				case 1:
					error = "El periodo se encuenta bloqueado para ingresar un asiento contable.";
					break;
				case 2:
					error = "No se encontro registro para el año.";
					break;
				case 3:
					error = "Se deben tener como minimo dos cuentas para el asiento contable";
					break;
				case 4:
					error = "No se encontro la cuenta para la compañia del año solicitado. Verificar los datos ingresados o si se debe crear la cuenta.";
					break;
				case 5:
					error = "La cuenta ingresada no es de movimiento.";
					break;
				case 6:
					error = "El centro de costo no existe ó no es de movimiento.";
					break;
				case 7:
					error = "El codigo del centro de costo no puede ser nulo.";
					break;
				case 8:
					error = "El tercero no existe.";
					break;
				case 9:
					error = "El Nit no puede ser nulo.";
					break;
				case 10:
					error = "El tipo de documento no existe ó no esta asociado a la compañia.";
					break;
				case 11:
					error = "El usuario no existe.";
					break;
				case 12:
					error = "Los campos " + fieldNames + " son requeridos.";
					break;
				case 13:
					error = "La compañia no existe.";
					break;
				case 14:
					error = "El nombre de la aplicación no existe.";
					break;
				case 15:
					error = "El documento no existe.";
					break;
				case 16:
					error = "La diferencia entre el valor Debito y el valor Credito debe ser 0. Debe revisar los valores de las cuentas.";
					break;

			}
			return error;
		}
	}
}
