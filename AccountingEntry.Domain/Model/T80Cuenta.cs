using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T80Cuenta
	{
        public string T80CodCia { get; set; }
        public short T80Agno { get; set; }
        
        [Key]
        public string T80CodCta { get; set; }
        public string T80CodRapido { get; set; }
        public string T80Naturaleza { get; set; }
        public string T80Nombre { get; set; }
        public string T80Movimiento { get; set; }
        public string T80CentroCosto { get; set; }
        public string T80Tercero { get; set; }
        public string T80CodTipoMoneda { get; set; }
        public string T80CodCtaAjuMoneda { get; set; }
        public string T80Dinamica { get; set; }
        public string T80CodCtaAjuCruce { get; set; }
        public string T80CodCtaAjuAplic { get; set; }
        public string T80ContabLocal { get; set; }
        public string T80ContabNIIF { get; set; }
        public string T80CodCtaNIIF { get; set; }
        public string T80NombreNIIF { get; set; }
    }
}
