using CategoryService.Domain.Models;

namespace CategoryService.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);
    Task<Category> Add(Category category);
    Task<Category> Update(Category category);
}
