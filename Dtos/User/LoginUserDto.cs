using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login.Dtos.User
{
    public class LoginUserDto
    {
        public string UsernameOrEmail {get;set;} = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}