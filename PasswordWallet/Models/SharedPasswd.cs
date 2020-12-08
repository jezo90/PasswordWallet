using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class SharedPasswd
    {
        [Key]
        public int Id { get; set; }
        public int PasswdId { get; set; }
        public int UserOwnerId { get; set; }
        public int UserSharedId { get; set; }


    }
}
