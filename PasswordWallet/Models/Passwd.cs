using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class Passwd
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Login { get; set; }
        public string Password { get; set; }
        public string WebAddress { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

    }
}
