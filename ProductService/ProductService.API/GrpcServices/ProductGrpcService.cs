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
        Console.WriteLine("Hello");
        var products = await _productService.GetProductsByCategoryIdAsync(request.CategoryId);

        var response = new GetProductsByCategoryIdResponse();
        response.Products.AddRange(products.Select(p => new ProductItem
        {
            ProductId = (int)p.Id!,
            Name = p.Name
        }));

        return response;
    }

    public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        var result = await _productService.DeleteProductAsync(request.ProductId);
        return new DeleteProductResponse { Success = result };
    }
}
