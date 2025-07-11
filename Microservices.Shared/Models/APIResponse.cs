using System.Net;

namespace Microservices.Shared;

public class APIResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public object? Result { get; set; }

    public Dictionary<string, List<string>>? ValidationMessages { get; set; }
}
