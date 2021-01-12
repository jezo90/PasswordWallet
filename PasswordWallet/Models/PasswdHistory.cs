using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class PasswdHistory
    {
        [Key]
        public int Id { get; set; }

        public string OldPasswd { get; set; }
        public string NewPasswd { get; set; }

        public int UserId { get; set; }
        public int PasswdId { get; set; }

        public DateTime Time { get; set; }
    }
}
