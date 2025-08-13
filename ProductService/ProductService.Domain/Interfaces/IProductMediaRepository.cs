using ProductService.Domain.Models;

namespace ProductService.Domain.Interfaces;

public interface IProductMediaRepository
{
    Task<string?> GetProductImageById(int productId);

    Task<List<ProductMedia>> GetByProductIdAsync(int productId);

    Task AddAsync(ProductMedia productMedia);

    Task AddRangeAsync(List<ProductMedia> productMedias);

    Task UpdateAsync(ProductMedia productMedia);

    Task UpdateRangeAsync(List<ProductMedia> medias);

    Task<List<ProductMedia>> GetByProductIdsAsync(List<int> productIds);
}
