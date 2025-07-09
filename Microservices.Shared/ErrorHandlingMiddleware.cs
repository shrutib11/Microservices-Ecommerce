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
            _response = exception switch
            {
                InvalidCredentialException invalidCredEx => ApiResponseHelper.Error(invalidCredEx.Message, HttpStatusCode.BadRequest),
                InvalidOperationException invalidOpEx => ApiResponseHelper.Error(invalidOpEx.Message, HttpStatusCode.BadRequest),
                ArgumentNullException argNullEx => ApiResponseHelper.Error(argNullEx.Message, HttpStatusCode.BadRequest),
                ArgumentException argEx => ApiResponseHelper.Error(argEx.Message, HttpStatusCode.BadRequest),
                KeyNotFoundException keyNotFoundEx => ApiResponseHelper.Error(keyNotFoundEx.Message, HttpStatusCode.NotFound),
                Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx => ApiResponseHelper.Error("Database update error. Please check your input.", HttpStatusCode.Conflict),
                _ => ApiResponseHelper.Error("An unexpected error occurred. Please try again later.", HttpStatusCode.InternalServerError),
            };
            context.Response.StatusCode = (int)_response.StatusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(_response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
