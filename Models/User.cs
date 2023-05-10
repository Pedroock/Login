using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Login.Models
{
    public class User
    {
        public int Id {get;set;}
        [Unicode(false)]
        [MaxLength(50)]
        public string Username {get;set;} = string.Empty;
        [Unicode(false)]
        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;
        public byte[] PasswordHash {get;set;} = new byte[0];
        public byte[] PasswordSalt {get;set;} = new byte[0];
        [Unicode(false)]
        [MaxLength(3)]
        public string Role { get; set; } = string.Empty;
        [Unicode(false)]
        [MaxLength(6)]
        public string ValidationCode { get; set; } = string.Empty;
        public bool IsValidated { get; set; } = false;
    }
}