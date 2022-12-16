using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	
	public class T85Documento
    {
        public string T85CodCia {get; set;}
        public short T85Agno { get; set; }
        public string T85CodTipoDoc { get; set; }
        [Key]
        public int T85NumeroDoc { get; set; }
        public DateTime T85Fecha { get; set; }
        public string T85Concepto { get; set; }
        public string T85Anulado { get; set; }
        public string T85AppName { get; set; }
        public int T85NumeroDocAnul { get; set; }
        public string T85CodTipoOrigenDoc { get; set; }
        public int T85NumeroOrigenDoc { get; set; }
        public string T85ReferenciaOrigenDoc { get; set; }
        public string T85ContabLocal { get; set; }
        public string T85ContabNIIF { get; set; }
        public int T85NumRevelacion { get; set; }
        [NotMapped]
        public Byte[] T85TimeStamp { get; set; }
	}
}
