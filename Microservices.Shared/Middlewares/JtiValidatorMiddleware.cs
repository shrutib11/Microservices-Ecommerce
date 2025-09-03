// using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Microservices.Shared.Middlewares;

public class JtiValidatorMiddleware
{
    // private readonly RequestDelegate _next;
    // private readonly IMemoryCache _cache;

    // public JtiValidatorMiddleware(RequestDelegate next, IMemoryCache cache)
    // {
    //     _next = next;
    //     _cache = cache;
    // }

    // public async Task InvokeAsync(HttpContext context)
    // {
    //     if (context.User.Identity?.IsAuthenticated == true)
    //     {
    //         var useremail = context.User.FindFirst(ClaimTypes.Email)?.Value;
    //         var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

    //         if (string.IsNullOrEmpty(useremail) || string.IsNullOrEmpty(jti))
    //         {
    //             throw new HttpStatusCodeException("Invalid token - missing claims.",HttpStatusCode.Unauthorized);
    //         }

    //         if (!_cache.TryGetValue($"user:{useremail}:jti", out string? cachedJti) || cachedJti != jti)
    //         {
    //             throw new HttpStatusCodeException("Token expired or invalid.",HttpStatusCode.Unauthorized);
    //         }
    //     }

    //     await _next(context);
    // }

    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public JtiValidatorMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(jti))
            {
                throw new HttpStatusCodeException("Invalid token - missing claims.", HttpStatusCode.Unauthorized);
            }

            var cacheKey = $"user:{userEmail}:jti";
            var cachedJti = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedJti) || cachedJti != jti)
            {
                throw new HttpStatusCodeException("Token expired or invalid.", HttpStatusCode.Unauthorized);
            }
        }

        await _next(context);
    }
}

