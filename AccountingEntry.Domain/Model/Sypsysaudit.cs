using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class Sypsysaudit
	{
        public DateTime FechaAud { get; set; }
        public int IdUsr { get; set; }
        public string TablaAud { get; set; }
        public short TransAud { get; set; }
        public string DescAud { get; set; }
        [Key]
        public string ComputerAud { get; set; }
        [NotMapped]
        public decimal ConsecAud { get; set; }
    }
}
