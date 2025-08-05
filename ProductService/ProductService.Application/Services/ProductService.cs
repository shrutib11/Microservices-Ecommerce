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
        var productMediaList = new List<ProductMedia>();

        if (productDto.ProductMedias != null && productDto.ProductMedias.Any())
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            foreach (var mediaDto in productDto.ProductMedias)
            {
                if (mediaDto.MediaFile == null)
                    throw new ArgumentException("Media file cannot be null.");

                string filename = ImageHelper.SaveImageWithName(mediaDto.MediaFile, productDto.Name, rootPath);

                productMediaList.Add(new ProductMedia
                {
                    MediaType = mediaDto.MediaType,
                    MediaUrl = filename,
                    DisplayOrder = mediaDto.DisplayOrder
                });
            }
        }

        var savedProduct = await _productRepository.AddProductWithMediaAsync(product, productMediaList);

        var resultDto = _mapper.Map<ProductDto>(savedProduct.Product);
        resultDto.ProductMedias = _mapper.Map<List<ProductMediasDto>>(savedProduct.MediaList);

        return resultDto;
    }


    public async Task<ProductDto?> UpdateProductAsync(ProductDto productDto)
    {
        var product = await _productRepository.GetByIdAsync(Convert.ToInt32(productDto.Id));
        if (product == null) return null;

        _mapper.Map(productDto, product);

        // if (productDto.ProductImageFile != null)
        // {
        //     var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        //     // product.ProductImage = ImageHelper.SaveImageWithName(productDto.ProductImageFile, productDto.Name, rootPath);
        // }

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

    public async Task<ProductDto?> UpdateRatings(int productId, decimal avgRating, int totalRatings)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return null;

        product.AvgRating = avgRating;
        product.TotalReviews = totalRatings;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return _mapper.Map<ProductDto>(updatedProduct);
    }
}
