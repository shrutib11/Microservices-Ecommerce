using ProductService.Domain.Models;

namespace ProductService.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductAsync();
    Task<Product?> GetByIdAsync(int id);
}
