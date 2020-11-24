using System;
using System.ComponentModel.DataAnnotations;

namespace PasswordWallet.Models
{
    public class AddressIP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Address { get; set; }

        public DateTime IpBlockDate { get; set; }

        [Required]
        public int Correct { get; set; }

        [Required]
        public int Incorrect { get; set; }


    }
}
