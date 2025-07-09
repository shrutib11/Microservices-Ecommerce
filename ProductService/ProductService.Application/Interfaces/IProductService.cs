using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> AddProductAsync(ProductDto productDto);
    Task<ProductDto?> UpdateProductAsync(ProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
}
