using Microsoft.EntityFrameworkCore;
using System;
using AccountingEntry.Domain.Model;

namespace AccountingEntry.Infrastructure.Persistence
{
	public class PimisysContext: DbContext
	{
		public PimisysContext(DbContextOptions<PimisysContext> options) : base(options) { }

		public DbSet<Registry> REGISTRY { get; set; }
		public DbSet<Sypsysaudit> SYPSYSAUDIT { get; set; }
		public DbSet<SypsysUsers> SYPSYSUSERS { get; set; }
		public DbSet<SysApplication> SYSAPPLICATION { get; set; }
		public DbSet<T01Cia> T01CIA { get; set; }
		public DbSet<T80Cuenta> T80CUENTA { get; set; }
		public DbSet<T84CentroCosto> T84CENTROCOSTO { get; set; }
		public DbSet<T03Tercero> T03TERCERO { get; set; }
		public DbSet<T85Documento> T85DOCUMENTO { get; set; }
		public DbSet<T86TipoDocumentoCia> T86TIPODOCUMENTOCIA { get; set; }
		public DbSet<T87Movimiento> T87MOVIMIENTO { get; set; }
		public DbSet<T88TotalCuenta> T88TOTALCUENTA { get; set; }
		public DbSet<T89TotalCentroCosto> T89TOTALCENTROCOSTO { get; set; }
		public DbSet<T90TotalTercero> T90TOTALTERCERO { get; set; }
	}
}
