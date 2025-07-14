using CategoryService.Application.Interfaces;
using Grpc.Core;
using Microservices.Shared.Protos;


namespace CategoryService.API.GrpcServices;

public class CategoryGrpcService : Category.CategoryBase
{
    private readonly ICategoryService _categoryService;

    public CategoryGrpcService(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public override async Task<GetCategoryByIdResponse> GetCategoryById(GetCategoryByIdRequest request, ServerCallContext context)
    {
        var category = await _categoryService.GetCategoryById(request.CategoryId);
        if (category == null)
        {
            return new GetCategoryByIdResponse { IsFound = false };
        }

        return new GetCategoryByIdResponse
        {
            CategoryId = category.Id,
            Name = category.Name,
            IsFound = true
        };
    }
}
