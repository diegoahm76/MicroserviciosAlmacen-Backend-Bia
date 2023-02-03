using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingEntry.API.BindingModel
{
	public class DeleteDocumentRequest
	{
		public string CodCia { get; set; }
		public DateTime FechaTransaccion { get; set; } //FechaActual
        public string CodTipoDoc { get; set; }
		public int NumeroDoc { get; set; }
		public string AppName { get; set; }
		public int IdUsr { get; set; }
		public string ComputerAud { get; set; }
	}
}
