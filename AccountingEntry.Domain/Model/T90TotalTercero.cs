using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T90TotalTercero
	{
        public string T90CodCia { get; set; }
        public short T90Agno { get; set; }
        public string T90CodCta { get; set; }
        [Key]
        public string T90Nit { get; set; }
        public Byte T90Mes { get; set; }
        public decimal T90MovDebitoLocal { get; set; }
        public decimal T90MovCreditoLocal { get; set; }
        public decimal T90MovDebitoNIIF { get; set; }
        public decimal T90MovCreditoNIIF { get; set; }
    }
}
