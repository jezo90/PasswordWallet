using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class ActionType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Action { get; set; }

        public DateTime Time { get; set; }

        public int UserId { get; set; }

    }
}
