using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T87Movimiento
	{
        public string T87CodCia { get; set; }
        public short T87Agno { get; set; }
        public string T87CodTipoDoc { get; set; }
        public int T87NumeroDoc { get; set; }
        public string T87CodCta { get; set; }
        [Key]
        public int T87ConsecCta { get; set; }
        public DateTime T87Fecha { get; set; }
        public string T87CodCentro { get; set; }
        public string T87Nit { get; set; }
        public string T87Referencia { get; set; }
        public string T87Detalle { get; set; }
        public string T87CodTipoDocCruce { get; set; }
        public string T87NumeroDocCruce { get; set; }
        public decimal T87ValorBase { get; set; }
        public decimal T87ValorDebito { get; set; }
        public decimal T87ValorCredito { get; set; }
        public string T87ContabLocal { get; set; }
        public string T87ContabNIIF { get; set; }
        public int T87NumRevelacion { get; set; }
    }
}
