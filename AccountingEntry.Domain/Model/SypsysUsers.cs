using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AccountingEntry.Domain.Model
{
	public class SypsysUsers
	{
        [Key]
        public int IdUsr { get; set; }
        public string UserName { get; set; }
        public string LongUserName { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
        public short PasswordAge { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public string NextLoginPasswordChange { get; set; }
        public string DBAdministrator { get; set; }
        public string AccessAllCias { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Active { get; set; }
        public string Deleted { get; set; }
    }
}
