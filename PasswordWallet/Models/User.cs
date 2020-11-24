using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nickname { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Salt { get; set; }

        [Required]
        public string isPasswordKeptAsHash { get; set; }

        public bool IsAccountBlocked { get; set; }
        public DateTime AccountBlockDate { get; set; }

        public virtual ICollection<Passwd> Passwds { get; set; }
        public virtual ICollection<LoginAttempt> LoginAttempts { get; set; }

    }
}
