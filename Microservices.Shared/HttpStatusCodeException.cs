using System.Net;

namespace Microservices.Shared;

public class HttpStatusCodeException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public HttpStatusCodeException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        StatusCode = statusCode;
    }
}