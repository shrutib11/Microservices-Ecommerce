using System.Security.Authentication;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Net;


namespace Microservices.Shared;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    protected APIResponse _response = new();

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            
            if (exception is HttpStatusCodeException httpEx)
            {
                _response = ApiResponseHelper.Error(httpEx.Message, httpEx.StatusCode);
            }
            else
            {
                _response = ApiResponseHelper.Error("An unexpected error occurred. Please try again later.", HttpStatusCode.InternalServerError);
            }

            context.Response.StatusCode = (int)_response.StatusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(_response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
