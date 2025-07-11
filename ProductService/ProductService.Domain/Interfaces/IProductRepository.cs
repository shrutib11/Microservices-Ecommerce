using ProductService.Domain.Models;

namespace ProductService.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task<Product?> UpdateAsync(Product product);
    Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId);
}
