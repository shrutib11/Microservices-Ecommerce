using System.Net;
using Microservices.Shared;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryServiceProxy _categoryServiceProxy;
    public ProductController(IProductService productService, ICategoryServiceProxy categoryServiceProxy)
    {
        _categoryServiceProxy = categoryServiceProxy;
        _productService = productService;
    }

    [HttpGet("GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(ApiResponseHelper.Success(products, HttpStatusCode.OK));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null || product.Id == 0)
        {
            return NotFound(ApiResponseHelper.Error("Product Not Found", HttpStatusCode.NotFound));
        }

        return Ok(ApiResponseHelper.Success(product, HttpStatusCode.OK));
    }

    [HttpPost("Add")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProduct([FromForm] ProductDto productDto)
    {
        var addedProduct = await _productService.AddProductAsync(productDto);
        return Ok(ApiResponseHelper.Success(addedProduct, HttpStatusCode.Created));
    }

    [HttpPut("Update")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct([FromForm] ProductDto productDto)
    {
        var updatedProduct = await _productService.UpdateProductAsync(productDto);
        if (updatedProduct == null)
        {
            return NotFound(ApiResponseHelper.Error("Product not found", HttpStatusCode.NotFound));
        }

        return Ok(ApiResponseHelper.Success(updatedProduct, HttpStatusCode.OK));
    }

    [HttpPatch("Delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var isDeleted = await _productService.DeleteProductAsync(id);
        if (!isDeleted)
        {
            return NotFound(ApiResponseHelper.Error("Product not found", HttpStatusCode.NotFound));
        }

        return Ok(ApiResponseHelper.Success(HttpStatusCode.NoContent));
    }

    [HttpGet("GetByCategory/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetProductsByCategory(int CategoryId)
    {
        var categoryExists = await _categoryServiceProxy.CategoryExistsAsync(CategoryId);
        if (!categoryExists)
        {
            throw new HttpStatusCodeException("Category not found", HttpStatusCode.NotFound);
        }
        var products = await _productService.GetProductsByCategoryIdAsync(CategoryId);
        return Ok(ApiResponseHelper.Success(products, HttpStatusCode.OK));
    }
}
