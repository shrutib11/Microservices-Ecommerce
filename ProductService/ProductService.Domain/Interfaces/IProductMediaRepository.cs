using ProductService.Domain.Models;

namespace ProductService.Domain.Interfaces;

public interface IProductMediaRepository
{
    Task<string?> GetProductImageById(int productId);

    Task<List<ProductMedia>> GetByProductIdAsync(int productId);

    Task AddAsync(ProductMedia productMedia);

    Task UpdateAsync(ProductMedia productMedia);
}
