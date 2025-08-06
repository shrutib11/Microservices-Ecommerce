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
    private readonly IProductMediaRepository _productMediaRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository productRepository, IProductMediaRepository productMediaRepository, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productMediaRepository = productMediaRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        List<Product> products = await _productRepository.GetAllProductAsync();
        var productviewModels = _mapper.Map<List<ProductDto>>(products);

        foreach (var p in productviewModels)
        {
            var productMediaViewModel = await _productMediaRepository.GetByProductIdAsync(p.Id);
            p.ProductImage = productMediaViewModel.Where(p => p.DisplayOrder == 1).Select(p => p.MediaUrl).FirstOrDefault();
            p.ProductMedias = _mapper.Map<List<ProductMediasDto>>(productMediaViewModel);
        }
        return productviewModels;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        var productMedias = await _productMediaRepository.GetByProductIdAsync(id);
        if (product == null) return null;

        var productViewModel = _mapper.Map<ProductDto>(product);
        productViewModel.ProductImage = productMedias.Where(p => p.DisplayOrder == 1).Select(p => p.MediaUrl).FirstOrDefault();
        productViewModel.ProductMedias = _mapper.Map<List<ProductMediasDto>>(productMedias);
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
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var product = await _productRepository.GetByIdAsync(Convert.ToInt32(productDto.Id));
            if (product == null) return null;

            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.Now;
            await _productRepository.UpdateAsync(product);

            //When no change in media
            var shouldProcessMedia = productDto.ProductMedias != null
                    && productDto.ProductMedias.Any(m => m.MediaFile != null || m.Id != 0 || !string.IsNullOrEmpty(m.MediaUrl));


            if (shouldProcessMedia && productDto.ProductMedias != null)
            {
                var existingMedias = await _productMediaRepository.GetByProductIdAsync(product.Id);

                var incomingMediaIds = productDto.ProductMedias
                    .Where(pm => pm.Id != 0).Select(pm => pm.Id).ToList();

                foreach (var media in existingMedias)
                {
                    if (!incomingMediaIds.Contains(media.Id))
                    {
                        media.IsDeleted = true;
                        await _productMediaRepository.UpdateAsync(media);
                    }
                }

                // Update or add media
                foreach (var mediaDto in productDto.ProductMedias)
                {
                    var id = mediaDto.Id;
                    var formFile = mediaDto.MediaFile;
                    var mediaType = mediaDto.MediaType;
                    var displayOrder = mediaDto.DisplayOrder;

                    if (id != 0)
                    {
                        var existingMedia = existingMedias.FirstOrDefault(m => m.Id == id);
                        if (existingMedia != null)
                        {
                            existingMedia.MediaType = mediaType;
                            existingMedia.DisplayOrder = displayOrder;

                            if (formFile != null)
                            {
                                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                                existingMedia.MediaUrl = ImageHelper.SaveImageWithName(formFile, product.Name, rootPath);
                            }

                            await _productMediaRepository.UpdateAsync(existingMedia);
                        }
                    }
                    else
                    {
                        var newMedia = new ProductMedia
                        {
                            ProductId = product.Id,
                            MediaType = mediaType,
                            DisplayOrder = displayOrder
                        };

                        if (formFile != null)
                        {
                            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                            newMedia.MediaUrl = ImageHelper.SaveImageWithName(formFile, product.Name, rootPath);
                        }

                        await _productMediaRepository.AddAsync(newMedia);
                    }
                }
            }

            await transaction.CommitAsync();

            var savedProductMedias = await _productMediaRepository.GetByProductIdAsync(product.Id);

            var resultDto = _mapper.Map<ProductDto>(product);
            resultDto.ProductImage = savedProductMedias.Where(p => p.DisplayOrder == 1).Select(p => p.MediaUrl).FirstOrDefault();
            resultDto.ProductMedias = _mapper.Map<List<ProductMediasDto>>(savedProductMedias);

            return resultDto;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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