using CategoryService.Application.DTOs;
using CategoryService.Domain.Models;

namespace CategoryService.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();

    Task<CategoryDto?> GetCategoryById(int id);

    Task<CategoryDto> Add(CategoryDto categoryDto);

    Task<CategoryDto> Update(CategoryDto categoryDto);

    Task Delete(int id);
}
