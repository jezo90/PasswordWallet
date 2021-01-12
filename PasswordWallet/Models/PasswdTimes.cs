using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class PasswdTimes
    {
        public int Id { get; set; }

        public string OldPasswd { get; set; }
        public string NewPasswd { get; set; }

        public int UserId { get; set; }
        public int PasswdId { get; set; }

        public string Time { get; set; }
        public string Date { get; set; }
    }
}
