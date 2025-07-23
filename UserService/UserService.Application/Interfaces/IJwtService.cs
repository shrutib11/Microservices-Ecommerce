using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UserService.Application.Interfaces
{
    public interface IJwtService
    {
        string generateJwtToken(string email, string role, int userId);
        public ClaimsPrincipal? ValidateTokens(string token);
    }
}