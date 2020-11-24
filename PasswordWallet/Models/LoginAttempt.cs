using System;
using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.Models
{
    public class LoginAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AddressIp { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool Successful { get; set; }

        [Required]
        public int Attempt { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

    }
}
