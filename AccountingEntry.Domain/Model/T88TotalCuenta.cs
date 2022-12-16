using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T88TotalCuenta
	{
        public string T88CodCia { get; set; }
        public short T88Agno { get; set; }
        [Key]
        public string T88CodCta { get; set; }
        public Byte T88Mes { get; set; }
        public decimal T88MovDebitoLocal { get; set; }
        public decimal T88MovCreditoLocal { get; set; }
        public decimal T88MovDebitoNIIF { get; set; }
        public decimal T88MovCreditoNIIF { get; set; }
    }
}
