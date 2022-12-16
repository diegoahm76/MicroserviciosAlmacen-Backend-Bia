using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T86TipoDocumentoCia
	{
		public string T86CodCia { get; set; }
		[Key]
		public string T86CodTipoDoc { get; set; }
		public string T86Nombre { get; set; }
		public string T86Observacion { get; set; }
		public string T86Delete { get; set; }
	}
}
