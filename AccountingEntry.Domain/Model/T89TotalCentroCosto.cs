using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T89TotalCentroCosto
	{
        public string T89CodCia { get; set; }
        public short T89Agno { get; set; }
        [Key]
        public string T89CodCta { get; set; }
        public string T89CodCentro { get; set; }
        public Byte T89Mes { get; set; }
        public decimal T89MovDebitoLocal { get; set; }
        public decimal T89MovCreditoLocal { get; set; }
        public decimal T89MovDebitoNIIF { get; set; }
        public decimal T89MovCreditoNIIF { get; set; }
    }
}
