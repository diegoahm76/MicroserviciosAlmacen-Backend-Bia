using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class SysApplication
	{
        public string AppName { get; set; }
        public string AppTitle { get; set; }
        public string LongName { get; set; }
        [Key]
        public byte IdApp { get; set; }
        public string IsAppExe { get; set; }
        public string Active { get; set; }
        public string IsExternal { get; set; }
    }
}
