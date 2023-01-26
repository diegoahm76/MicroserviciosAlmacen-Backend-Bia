using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingEntry.API.BindingModel
{
	public class DocumentTransaction
	{
		public string T85CodCia { get; set; }
		public short T85Agno { get; set; }
		public string T85CodTipoDoc { get; set; }
		public int T85NumeroDoc { get; set; }
	}
}
