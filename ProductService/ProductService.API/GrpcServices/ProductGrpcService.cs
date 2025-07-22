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
        if (product == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));

        return new GetProductResponse
        {
            Product = new ProductItem
            {
                ProductId = (int)product.Id!,
                Name = product.Name,
                Image = product.ProductImage
            }
        };
    }
}
