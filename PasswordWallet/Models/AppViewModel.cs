using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class AppViewModel
    {
        public Passwd Passwd { get; set; }
        public User User { get; set; }
        public string Logged { get; set; }
    }
}
