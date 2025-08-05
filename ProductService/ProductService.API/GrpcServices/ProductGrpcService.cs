using ProductService.Application.Interfaces;
using Grpc.Core;
using Microservices.Shared.Protos;


namespace ProductService.API.GrpcServices;

public class ProductGrpcService : Product.ProductBase
{
    private readonly IProductService _productService;

    public ProductGrpcService(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<GetProductsByCategoryIdResponse> GetProductsByCategoryId(GetProductsByCategoryIdRequest request, ServerCallContext context)
    {
        var products = await _productService.GetProductsByCategoryIdAsync(request.CategoryId);

        var response = new GetProductsByCategoryIdResponse();
        response.Products.AddRange(products.Select(p => new ProductItem
        {
            ProductId = (int)p.Id!,
            Name = p.Name
        }));

        return response;
    }

    public override async Task<DeleteProductResponse> DeleteProduct(ProductRequest request, ServerCallContext context)
    {
        var result = await _productService.DeleteProductAsync(request.ProductId);
        return new DeleteProductResponse { Success = result };
    }

    public override async Task<GetProductResponse> GetProductById(ProductRequest request, ServerCallContext context)
    {
        var product = await _productService.GetProductByIdAsync(request.ProductId);

        var response = new GetProductResponse
        {
            Product = product == null
           ? new ProductItem { IsFound = false }
           : new ProductItem
           {
               ProductId = (int)product.Id!,
               Name = product.Name,
               Image = product.ProductMedias.Where(p => p.DisplayOrder == 1 && !p.IsDeleted).Select(p => p.MediaUrl).FirstOrDefault(),
               IsFound = true
           }
        };

        return response;
    }

    public override async Task<UpdateProductRatingsResponse> UpdateRatingInfo(UpdateProductRatingsRequest request, ServerCallContext context)
    {
        var product = await _productService.UpdateRatings(request.ProductId, (decimal)request.AvgRating, request.TotalRatings);
        if (product == null) return new UpdateProductRatingsResponse { Success = false };

        return new UpdateProductRatingsResponse { Success = true };
    }
}
