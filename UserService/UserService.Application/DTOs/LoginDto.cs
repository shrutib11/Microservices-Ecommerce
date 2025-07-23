using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Application.DTOs
{
    public class LoginDto
    {
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}