namespace ProductService.Application.Interfaces;

public interface ICategoryServiceProxy
{
    Task<bool> CategoryExistsAsync(int categoryId);
}
