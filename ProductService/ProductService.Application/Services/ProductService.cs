using AutoMapper;
using Microservices.Shared.Helpers;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Enums;
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

        var productIds = productviewModels.Select(p => p.Id ?? 0).ToList();
        var allMedias = await _productMediaRepository.GetByProductIdsAsync(productIds);

        foreach (var p in productviewModels)
        {
            var medias = allMedias.Where(m => m.ProductId == p.Id).ToList();
            p.ProductImage = medias.Where(pm => pm.DisplayOrder == 1).Select(pm => pm.MediaUrl).FirstOrDefault();
            p.ProductMedias = _mapper.Map<List<ProductMediasDto>>(medias);
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
        var firstMedia = (productDto.ProductMedias
            ?.FirstOrDefault(m => m.DisplayOrder == 1 && !m.IsDeleted)) ?? throw new ArgumentException("First media is required and must not be deleted.");

        if (firstMedia.MediaType != MediaType.Image)
            throw new ArgumentException("The first media must be an image. Videos are not allowed.");

        if (productDto.ProductMedias == null || !productDto.ProductMedias.Any())
            throw new ArgumentException("At least one media file is required.");

        var product = _mapper.Map<Product>(productDto);
        product.AvgRating = (decimal?)0.0;
        product.TotalReviews = 0;

        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var productMediaList = productDto.ProductMedias.Select(mediaDto =>
        {
            if (mediaDto.MediaFile == null)
                throw new ArgumentException("Media file cannot be null.");

            string filename = ImageHelper.SaveImageWithName(mediaDto.MediaFile, productDto.Name, rootPath);

            return new ProductMedia
            {
                MediaType = mediaDto.MediaType,
                MediaUrl = filename,
                DisplayOrder = mediaDto.DisplayOrder
            };
        }).ToList();

        var savedProduct = await _productRepository.AddProductWithMediaAsync(product, productMediaList);

        var resultDto = _mapper.Map<ProductDto>(savedProduct.Product);
        resultDto.ProductImage = savedProduct.MediaList.Where(p => p.DisplayOrder == 1).Select(p => p.MediaUrl).FirstOrDefault();
        resultDto.ProductMedias = _mapper.Map<List<ProductMediasDto>>(savedProduct.MediaList);

        return resultDto;
    }

    public async Task<ProductDto?> UpdateProductAsync(ProductDto productDto)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto));

        var firstMedia = productDto.ProductMedias?
            .FirstOrDefault(m => m.DisplayOrder == 1 && !m.IsDeleted);

        if (firstMedia != null && firstMedia.MediaType != MediaType.Image)
            throw new ArgumentException("The first media must be of type Image. Videos are not allowed.");

        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var product = await _productRepository.GetByIdAsync(Convert.ToInt32(productDto.Id));
            if (product == null) return null;

            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.Now;
            await _productRepository.UpdateAsync(product);

            if (productDto.ProductMedias?.Any() == true)
            {
                var existingMedias = await _productMediaRepository.GetByProductIdAsync(product.Id);
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // Mark deleted medias
                var incomingIds = productDto.ProductMedias.Where(m => m.Id != 0).Select(m => m.Id);
                foreach (var media in existingMedias.Where(em => !incomingIds.Contains(em.Id)))
                {
                    media.IsDeleted = true;
                    media.UpdatedAt = DateTime.Now;

                    var oldFilePath = Path.Combine(rootPath, media.MediaUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }
                await _productMediaRepository.UpdateRangeAsync(existingMedias.Where(m => m.IsDeleted).ToList());

                // Update 
                foreach (var mediaDto in productDto.ProductMedias.Where(m => m.Id != 0))
                {
                    var existingMedia = existingMedias.FirstOrDefault(m => m.Id == mediaDto.Id);
                    if (existingMedia != null)
                    {
                        existingMedia.MediaType = mediaDto.MediaType;
                        existingMedia.DisplayOrder = mediaDto.DisplayOrder;

                        if (mediaDto.MediaFile != null)
                        {
                            var oldFilePath = Path.Combine(rootPath, existingMedia.MediaUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                            existingMedia.MediaUrl = ImageHelper.SaveImageWithName(mediaDto.MediaFile, product.Name, rootPath);
                            if (File.Exists(oldFilePath))
                                File.Delete(oldFilePath);
                        }

                        existingMedia.UpdatedAt = DateTime.Now;
                    }
                }
                await _productMediaRepository.UpdateRangeAsync(existingMedias.Where(m => !m.IsDeleted).ToList());

                // Add
                var newMedias = productDto.ProductMedias
                    .Where(m => m.Id == 0)
                    .Select(m => new ProductMedia
                    {
                        ProductId = product.Id,
                        MediaType = m.MediaType,
                        DisplayOrder = m.DisplayOrder,
                        MediaUrl = m.MediaFile != null
                            ? ImageHelper.SaveImageWithName(m.MediaFile, product.Name, rootPath)
                            : throw new ArgumentNullException("Media file cannot be empty", nameof(m.MediaFile))
                    }).ToList();

                if (newMedias.Any())
                    await _productMediaRepository.AddRangeAsync(newMedias);
            }

            await transaction.CommitAsync();

            var savedProductMedias = await _productMediaRepository.GetByProductIdAsync(product.Id);

            var resultDto = _mapper.Map<ProductDto>(product);
            resultDto.ProductImage = savedProductMedias.FirstOrDefault(p => p.DisplayOrder == 1)?.MediaUrl;
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
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            var productMedias = await _productMediaRepository.GetByProductIdAsync(id);
            productMedias.ForEach(pm => pm.IsDeleted = true);
            await _productMediaRepository.UpdateRangeAsync(productMedias);

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.Now;
            await _productRepository.UpdateAsync(product);

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
    {
        var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);
        var productViewModels = _mapper.Map<List<ProductDto>>(products);

        var productIds = productViewModels.Select(p => p.Id ?? 0).ToList();
        var allMedias = await _productMediaRepository.GetByProductIdsAsync(productIds);

        foreach (var p in productViewModels)
        {
            var medias = allMedias.Where(m => m.ProductId == p.Id).ToList();
            p.ProductImage = medias.Where(pm => pm.DisplayOrder == 1).Select(pm => pm.MediaUrl).FirstOrDefault();
            p.ProductMedias = _mapper.Map<List<ProductMediasDto>>(medias);
        }

        return productViewModels;
    }

    public async Task<List<ProductDto>> GetProductBySearchAsync(string searchTerm)
    {
        var products = await _productRepository.GetProductBySearch(searchTerm);
        var productviewModels = _mapper.Map<List<ProductDto>>(products);

        var productIds = productviewModels.Select(p => p.Id ?? 0).ToList();
        var allMedias = await _productMediaRepository.GetByProductIdsAsync(productIds);

        foreach (var p in productviewModels)
        {
            var medias = allMedias.Where(m => m.ProductId == p.Id).ToList();
            p.ProductImage = medias.Where(pm => pm.DisplayOrder == 1).Select(pm => pm.MediaUrl).FirstOrDefault();
            p.ProductMedias = _mapper.Map<List<ProductMediasDto>>(medias);
        }

        return productviewModels;
    }

    public async Task<ProductDto?> UpdateRatings(int productId, decimal avgRating, int totalRatings)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return null;

        product.AvgRating = avgRating;
        product.TotalReviews = totalRatings;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        var productDto = _mapper.Map<ProductDto>(updatedProduct);
        productDto.ProductImage = await _productMediaRepository.GetProductImageById(product.Id);
        return productDto;
    }
}
