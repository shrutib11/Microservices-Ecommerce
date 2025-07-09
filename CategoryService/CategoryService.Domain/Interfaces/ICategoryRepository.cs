using CategoryService.Domain.Models;

namespace CategoryService.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    void Update(Category category);

    Task SaveChangesAsync();
}
