using AutoMapper;
using CategoryService.Application.DTOs;
using CategoryService.Application.Interfaces;
using CategoryService.Domain.Interfaces;
using CategoryService.Domain.Models;
using Microservices.Shared.Helpers;

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

    public async Task<CategoryDto> Add(CategoryDto categoryDto)
    {
        var category = _mapper.Map<Category>(categoryDto);
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (categoryDto.CategoryFile != null)
        {
            category.CategoryImage = ImageHelper.SaveImageWithName(categoryDto.CategoryFile, categoryDto.Name, rootPath);
            category = await _categoryRepository.Add(category);
            return _mapper.Map<CategoryDto>(category);
        }
        else
            throw new ArgumentNullException("Category Image cannot be null");
    }

    public async Task<CategoryDto> Update(CategoryDto categoryDto)
    {
        Category? category = await _categoryRepository.GetByIdAsync(categoryDto.Id);
        if (category != null)
        {
            var existingImage = category.CategoryImage;
            _mapper.Map(categoryDto, category);

            if (categoryDto.CategoryFile != null)
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                category.CategoryImage = ImageHelper.SaveImageWithName(categoryDto.CategoryFile, categoryDto.Name, rootPath);
            }
            else
            {
                category.CategoryImage = existingImage; 
            }
            
            category.UpdatedAt = DateTime.Now;
            category = await _categoryRepository.Update(category);
            return _mapper.Map<CategoryDto>(category);
        }
        return new CategoryDto();
    }

    public async Task Delete(int id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
        {
            category.IsDeleted = true;
            category.UpdatedAt = DateTime.Now;
            await _categoryRepository.Update(category);
        }
    }
}
