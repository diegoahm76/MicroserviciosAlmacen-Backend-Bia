using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T84CentroCosto
	{
        public string T84CodCia { get; set; }
        [Key]
        public string T84CodCentro { get; set; }
        public string T84Nombre { get; set; }
        public string T84Observacion { get; set; }
        public string T84Movimiento { get; set; }
        public string T84Delete { get; set; }
    }
}
