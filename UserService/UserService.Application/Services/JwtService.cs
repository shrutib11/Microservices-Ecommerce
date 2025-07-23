using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Interfaces;

namespace UserService.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        public JwtService(IConfiguration configuration)
        {
            _key = configuration["Jwt:Key"] ?? "";
            _issuer = configuration["Jwt:Issuer"] ?? "";
            _audience = configuration["Jwt:Audience"] ?? "";
        }
        public string generateJwtToken(string email, string role, int userId)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var now = DateTime.UtcNow;
    var expires = now.AddDays(7);

    var claims = new[]
    {
        new Claim("email", email),
        new Claim("nameid", userId.ToString()),
        new Claim("role", role),
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        // DO NOT manually add Iat, Nbf, or Exp
    };

    var token = new JwtSecurityToken(
        issuer: _issuer,
        audience: _audience,
        claims: claims,
        notBefore: now,
        expires: expires,
        signingCredentials: creds
    );

    var tokenString = tokenHandler.WriteToken(token);

    // Debug info (optional)
    Console.WriteLine($"Issuer: {_issuer}");
    Console.WriteLine($"Audience: {_audience}");
    Console.WriteLine($"Expires (UTC): {expires}");

    return tokenString;
}

        public ClaimsPrincipal? ValidateTokens(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}