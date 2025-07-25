using AutoMapper;
using Microservices.Shared.Helpers;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Interfaces;
using ProductService.Domain.Models;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        List<Product> products = await _productRepository.GetAllProductAsync();
        var productviewModels = _mapper.Map<List<ProductDto>>(products);
        return productviewModels;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        var productViewModel = _mapper.Map<ProductDto>(product);
        return productViewModel;
    }

    public async Task<ProductDto> AddProductAsync(ProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        if (productDto.ProductImageFile == null)
        {
            throw new ArgumentNullException("Product Image cannot be null");
        }
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        product.ProductImage = ImageHelper.SaveImageWithName(productDto.ProductImageFile, productDto.Name, rootPath);
        var addedProduct = await _productRepository.AddAsync(product);
        return _mapper.Map<ProductDto>(addedProduct);
    }

    public async Task<ProductDto?> UpdateProductAsync(ProductDto productDto)
    {
        var product = await _productRepository.GetByIdAsync(Convert.ToInt32(productDto.Id));
        if (product == null) return null;

        var existingImage = product.ProductImage;
        _mapper.Map(productDto, product);

        if (productDto.ProductImageFile == null)
        {
            product.ProductImage = existingImage;
        }
        else
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            product.ProductImage = ImageHelper.SaveImageWithName(productDto.ProductImageFile, productDto.Name, rootPath);
        }

        product.UpdatedAt = DateTime.Now;
        var updatedProduct = await _productRepository.UpdateAsync(product);
        return _mapper.Map<ProductDto>(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return false;
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.Now;
        await _productRepository.UpdateAsync(product);
        return true;
    }

    public async Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
    {
        var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<List<ProductDto>> GetProductBySearchAsync(string searchTerm)
    {
        var products = await _productRepository.GetProductBySearch(searchTerm);
        return _mapper.Map<List<ProductDto>>(products);
    }
}
