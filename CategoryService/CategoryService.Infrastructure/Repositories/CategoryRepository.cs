using System.Threading.Tasks;
using CategoryService.Domain.Interfaces;
using CategoryService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly CategoryServiceDbContext _context;

    public CategoryRepository(CategoryServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.AsNoTracking().Where(c => c.IsDeleted == false && c.Id == id).FirstOrDefaultAsync();
    }

    public async Task Add(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
