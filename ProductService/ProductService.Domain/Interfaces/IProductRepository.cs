using ProductService.Domain.Models;

namespace ProductService.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<(Product Product, List<ProductMedia> MediaList)> AddProductWithMediaAsync(Product product, List<ProductMedia> productMediaList);
    Task<Product?> UpdateAsync(Product product);
    Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId);
    Task<List<Product>> GetProductBySearch(string searchTerm);
}
