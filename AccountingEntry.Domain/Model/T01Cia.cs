using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T01Cia
	{
        [Key]
        public string T01CodCia { get; set; }
        public string T01CodTipoCia { get; set; }
        public string T01Sigla { get; set; }
        public string T01Nombre { get; set; }
        public string T01Nit { get; set; }
        public string T01Direccion { get; set; }
        public string T01Telefono { get; set; }
        public string T01Fax { get; set; }
        public string T01EMail { get; set; }
        public string T01WebSite { get; set; }
        public string T01Observacion { get; set; }
        public byte[] T01Logotipo { get; set; }
        public string T01CodCiudad { get; set; }
        public string T01CodPostal { get; set; }
        public string T01Activa { get; set; }
    }
}
