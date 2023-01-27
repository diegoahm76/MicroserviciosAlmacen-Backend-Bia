using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingEntry.API.BindingModel
{
	public class CanceledDocumentRequest
	{
		public string CodCia { get; set; }
		public DateTime FechaDoc { get; set; }
		public DateTime FechaAnul { get; set; }
		public string CodTipoDoc { get; set; }
		public string CodTipoDocAnul { get; set; }
		public int NumeroDoc { get; set; }
		public string Concepto { get; set; }
		public string ConceptoAnul { get; set; }
		public string AppName { get; set; }
		public int IdUsr { get; set; }
		public string ComputerAud { get; set; }
	}
}
