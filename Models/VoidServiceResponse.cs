using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login.Models
{
    public class VoidServiceResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}