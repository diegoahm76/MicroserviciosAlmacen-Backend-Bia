using System;
using System.Collections.Generic;

namespace AccountingEntry.API.BindingModel
{
	public class RegistryInWareHouseRequest
	{
		//Propiedades comunes en varias tablas
		public string CodCia { get; set; } //REQUERIDO!
		public DateTime Fecha { get; set; } //REQUERIDO!
		public string CodCentro { get; set; }// Tener en cuenta que este campo puede estar dentro de la lista de cuentas en caso de que para cada una aplique un centro de costo
		public string Nit { get; set; }// // Tener en cuenta que este campo puede estar dentro de la lista de cuentas en caso de que para cada una aplique un tercero
		public string CodTipoDoc { get; set; } //REQUERIDO!

		//Propiedades de la tabla T85DOCUMENTO
		public string Concepto { get; set; } //REQUERIDO!
		public string AppName { get; set; } //REQUERIDO!

		//Propiedades de la tabla T87MOVIMIENTO
		public List<AccountRequest> Cuentas { get; set; }

		//Propiedades de la tabla SYPSYSAUDIT
		public int IdUsr { get; set; }
		public string ComputerAud { get; set; }
	}
}
