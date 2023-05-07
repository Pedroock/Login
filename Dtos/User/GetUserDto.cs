using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login.Dtos.User
{
    public class GetUserDto
    {
        public string Username {get;set;} = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsValidated { get; set; }
    }
}