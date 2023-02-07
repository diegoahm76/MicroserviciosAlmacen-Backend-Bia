using System;
using System.Collections.Generic;

namespace AccountingEntry.API.BindingModel
{
	public class RegistryInWareHouseRequest
	{
		//Propiedades comunes en varias tablas
		public string CodCia { get; set; } //REQUERIDO!
		public DateTime FechaNueva { get; set; } //REQUERIDO!
		public DateTime FechaAnterior { get; set; } //REQUERIDO SOLO PARA EDICIÓN
		public string CodCentro { get; set; }
		public string Nit { get; set; }
		public string CodTipoDoc { get; set; } //REQUERIDO!

		//Propiedades de la tabla T85DOCUMENTO
		public string Concepto { get; set; } //REQUERIDO!
		public string AppName { get; set; } //REQUERIDO!
		public int NumeroDoc { get; set; } //REQUERIDO!.

		//Propiedades de la tabla T87MOVIMIENTO
		public List<AccountRequest> Cuentas { get; set; }
		public string ReferenciaMov { get; set; } //OPCIONAL EN CASO DE REGISTRARSE ESTE DATO
		public string CodTipoDocCruceMov { get; set; } //OPCIONAL EN CASO DE REGISTRARSE ESTE DATO
		public string NumeroDocCruceMov { get; set; } //OPCIONAL EN CASO DE REGISTRARSE ESTE DATO

		//Propiedades de la tabla SYPSYSAUDIT
		public int IdUsr { get; set; }
		public string ComputerAud { get; set; }
	}
}
