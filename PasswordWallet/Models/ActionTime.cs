using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class ActionTime
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Date { get; set; }
        public int UserId { get; set; }
    }
}
