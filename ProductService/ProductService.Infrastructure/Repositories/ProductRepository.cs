using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Interfaces;
using ProductService.Domain.Models;
namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductAsync() => await _context.Products.Where(p => !p.IsDeleted).ToListAsync();

    public async Task<Product?> GetByIdAsync(int id) => await _context.Products.Where(p => p.Id == id && !p.IsDeleted).FirstOrDefaultAsync();

    public async Task<(Product Product, List<ProductMedia> MediaList)> AddProductWithMediaAsync(Product product, List<ProductMedia> productMediaList)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            foreach (var media in productMediaList)
            {
                media.ProductId = product.Id;
                media.CreatedAt = DateTime.Now;
            }

            await _context.ProductMedias.AddRangeAsync(productMediaList);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (product, productMediaList);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId) =>
        await _context.Products
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .ToListAsync();

    public async Task<List<Product>> GetProductBySearch(string searchTerm) => 
        await _context.Products
                .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) && !p.IsDeleted)
                .ToListAsync();
}