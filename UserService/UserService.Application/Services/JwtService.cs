using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IDistributedCache _cache;
        public JwtService(IConfiguration configuration, IDistributedCache cache)
        {
            _key = configuration["Jwt:Key"] ?? "";
            _issuer = configuration["Jwt:Issuer"] ?? "";
            _audience = configuration["Jwt:Audience"] ?? "";
            _cache = cache;
        }

        public string generateJwtToken(string email, string role, int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(15);

            var jti = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
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

            // await _cache.SetStringAsync($"user:{email}:jti",
            //      jti, new DistributedCacheEntryOptions
            //      {
            //          AbsoluteExpiration = new DateTimeOffset(expires)
            //      });



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