using System.Net;

namespace Microservices.Shared;

public class ApiResponseHelper
{
    public static APIResponse Success(object? result, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new APIResponse
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Result = result
        };
    }

    public static APIResponse Error(string errorMessage,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
    Dictionary<string, List<string>>? ValidationMessages = null)
    {
        return new APIResponse
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            ValidationMessages = ValidationMessages
        };
    }
}
