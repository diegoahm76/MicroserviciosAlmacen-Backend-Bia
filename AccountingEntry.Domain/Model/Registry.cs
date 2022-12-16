using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class Registry
	{
        [Key]
        public string AppName { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Etiqueta { get; set; }
        public short TipoDato { get; set; }
        public short Longitud { get; set; }
        public string Query { get; set; }
        public string Valor { get; set; }
        public int IdUsr { get; set; }
    }
}
