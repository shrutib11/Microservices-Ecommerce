using AutoMapper;
using CategoryService.Application.DTOs;
using CategoryService.Application.Interfaces;
using CategoryService.Domain.Interfaces;
using CategoryService.Domain.Models;
using Microservices.Shared;

namespace CategoryService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetCategoryById(int id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
            return _mapper.Map<CategoryDto>(category);

        return null;
    }

    public async Task Add(CategoryDto categoryDto)
    {
        var category = _mapper.Map<Category>(categoryDto);
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        category.CategoryImage = ImageHelper.SaveImageWithName(categoryDto.CategoryFile, categoryDto.Name, rootPath);
        await _categoryRepository.Add(category);
        await _categoryRepository.SaveChangesAsync();

    }

    public async Task Update(CategoryDto categoryDto)
    {
        Category? category = await _categoryRepository.GetByIdAsync(categoryDto.Id);
        if (category != null)
        {
            _mapper.Map(categoryDto, category);
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            category.CategoryImage = ImageHelper.SaveImageWithName(categoryDto.CategoryFile, categoryDto.Name, rootPath);
            category.UpdatedAt = DateTime.Now;
            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
        }
    }

    public async Task Delete(int id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
        {
            category.IsDeleted = true;
            category.UpdatedAt = DateTime.Now;
            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
        }
    }
}
