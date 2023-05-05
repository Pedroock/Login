using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login.Data;

namespace Login.Service.AuthService
{
    public class AuthService
    {
        private readonly DataContext _context;
        public AuthService(DataContext context)
        {
            _context = context;
        }
    }
}