using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Interfaces;
using ProductService.Domain.Models;

namespace ProductService.Infrastructure.Repositories;

public class ProductMediaRepository : IProductMediaRepository
{
    private readonly ProductDbContext _context;

    public ProductMediaRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetProductImageById(int productId) =>
        await _context.ProductMedias.Where(p => p.ProductId == productId && p.IsDeleted == false && p.DisplayOrder == 1).Select(p => p.MediaUrl).FirstOrDefaultAsync();

    public async Task<List<ProductMedia>> GetByProductIdAsync(int productId) =>
        await _context.ProductMedias.Where(p => p.ProductId == productId && p.IsDeleted == false).OrderBy(p => p.DisplayOrder).ToListAsync();

    public async Task AddAsync(ProductMedia productMedia)
    {
        await _context.ProductMedias.AddAsync(productMedia);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductMedia productMedia)
    {
        _context.ProductMedias.Update(productMedia);
        await _context.SaveChangesAsync();
    }
}
