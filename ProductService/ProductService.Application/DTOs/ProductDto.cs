using Microsoft.AspNetCore.Http;
using ProductService.Domain.Enums;

namespace ProductService.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public List<ProductMediasDto> ProductMedias { get; set; } = null!;
    public string? ProductImage { get; set; }
    public decimal? AvgRating { get; set; }
    public int? TotalReviews { get; set; }
}

public class ProductMediasDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public MediaType MediaType { get; set; }
    public IFormFile? MediaFile { get; set; }
    public string MediaUrl { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsDeleted { get; set; }
}