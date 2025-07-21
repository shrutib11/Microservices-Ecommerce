using Microsoft.AspNetCore.Http;

namespace CategoryService.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public IFormFile? CategoryFile { get; set; } = null!;
    public string? CategoryImage { get; set; }
}
