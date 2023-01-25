using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class T03Tercero
	{
        public string T03CodCia { get; set; }

        [Key]
        public string T03Nit { get; set; }
        public string T03CodCiudadCed { get; set; }
        public string T03CodRapido { get; set; }
        public string T03LibretaMil { get; set; }
        public string T03MatriProf { get; set; }
        public string T03Nombre { get; set; }
        public string T03PrimerApellido { get; set; }
        public string T03SegundoApellido { get; set; }
        public string T03PrimerNombre { get; set; }
        public string T03SegundoNombre { get; set; }
        public string T03CodPostal { get; set; }
        public string T03Direccion { get; set; }
        public string T03Telefono { get; set; }
        public string T03Fax { get; set; }
        public string T03EMail { get; set; }
        public string T03WebSite { get; set; }
        public string T03CodTipoSociedad { get; set; }
        public DateTime T03FechaIngreso { get; set; }
        public string T03CodCalifica { get; set; }
        public string T03Observacion { get; set; }
        public string T03CargoExterno { get; set; }
        public string T03NitRel { get; set; }
        public string T03CodTipoRegimen { get; set; }
        public byte T03TipoSeparaNombre { get; set; }
        public string T03CodDpto { get; set; }
        public string T03CodMpio { get; set; }
        public string T03CODCGN { get; set; }
        public string T03CODCTACONTABCAUSA { get; set; }
        public string T03CODACTRUT1 { get; set; }
        public string T03CODACTRUT { get; set; }
        public string T03CODACTRUT3 { get; set; }
        public string T03CodPais { get; set; }
        public string T03CodTipoDocumId { get; set; }
        public string T03CODRECIPROCA { get; set; }
        public string T03EntAseguradora { get; set; }
        public string T03CODENTCHIP { get; set; }
        public DateTime T03FECHANACIMIENTO { get; set; }
        public string T03GENERO { get; set; }
        public string T03ACTCERTIFPYG { get; set; }
        public DateTime T03FECHAACTWEBINFO { get; set; }
        public DateTime T03FECHASOLWEBINFO { get; set; }
        public string T03IPADDRACTSERV { get; set; }
        public string T03WEBPASSWORD { get; set; }
        public string T03ACTRECIBOSICAR { get; set; }
        public string T03ID_PCI_SIIF { get; set; }
    }
}
