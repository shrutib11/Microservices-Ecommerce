using System.Net.Http;
using System.Security.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;


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
            _response.IsSuccess = false;
            _response.Result = null;

            switch (exception)
            {
                case InvalidCredentialException invalidCredEx:
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { invalidCredEx.Message };
                    break;

                case InvalidOperationException invalidOpEx:
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { invalidOpEx.Message };
                    break;

                case ArgumentNullException argNullEx:
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { argNullEx.Message };
                    break;

                case ArgumentException argEx:
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { argEx.Message };
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { keyNotFoundEx.Message };
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx:
                    _response.StatusCode = System.Net.HttpStatusCode.Conflict;
                    _response.ErrorMessages = new List<string> { "Database update error. Please check your input." };
                    break;

                default:
                    _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    _response.ErrorMessages = new List<string> { "An unexpected error occurred. Please try again later." };
                    break;
            }

            context.Response.StatusCode = (int)_response.StatusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(_response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
