using System;
using System.Collections.Generic;

namespace AccountingEntry.Domain.Model.ModelQuery
{
	public class RegistryInWareHouse
	{
		//Propiedades comunes en varias tablas
		public string CodCia { get; set; }
		public DateTime FechaNueva { get; set; } //FechaActual
		public DateTime FechaAnterior { get; set; } //FechaAnterior
        public string CodCentro { get; set; }
		public string Nit { get; set; }
		public string CodTipoDoc { get; set; }
		public int NumRevelacion { get; set; }

		//Propiedades de la tabla T85DOCUMENTO
		public string Concepto { get; set; }
		public string Anulado { get; set; }
		public string AppName { get; set; }
		public int NumeroDocAnul { get; set; }
		public string CodTipoOrigenDoc { get; set; }
		public int NumeroOrigenDoc { get; set; }
		public int NumeroDoc { get; set; }
		public string ReferenciaOrigenDoc { get; set; }

		//Propiedades de la tabla T87MOVIMIENTO
		public List<Account> Cuentas { get; set; }
		public string ReferenciaMov { get; set; }
		public string CodTipoDocCruceMov { get; set; }
		public string NumeroDocCruceMov { get; set; }

		//Propiedades de la tabla SYPSYSAUDIT
		public int IdUsr { get; set; }
		public string ComputerAud { get; set; }
	}
}
