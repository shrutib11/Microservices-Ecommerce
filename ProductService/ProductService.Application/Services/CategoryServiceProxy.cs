using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Services;

public class CategoryServiceProxy : ICategoryServiceProxy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CategoryServiceProxy> _logger;

    public CategoryServiceProxy(HttpClient httpClient, ILogger<CategoryServiceProxy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/category/{categoryId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call CategoryService through API Gateway");
            return false;
        }
    }
}
