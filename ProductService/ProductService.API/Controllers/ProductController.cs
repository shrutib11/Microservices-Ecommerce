using System.Net;
using Microservices.Shared;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers;

// [ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly Category.CategoryClient _categoryClient;
    public ProductController(IProductService productService, Category.CategoryClient categoryClient)
    {
        _productService = productService;
        _categoryClient = categoryClient;
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
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(ms => ms.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                );
            return BadRequest(ApiResponseHelper.Error("Validation Failed", HttpStatusCode.BadRequest, errors));
        }
        
        // var response = await _categoryClient.GetCategoryByIdAsync(new GetCategoryByIdRequest
        // {
        //     CategoryId = productDto.CategoryId
        // });

        // if (!response.IsFound)
        // {
        //     return BadRequest(ApiResponseHelper.Error("Category does not exist.", HttpStatusCode.BadRequest));
        // }
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
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(ms => ms.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                );
            return BadRequest(ApiResponseHelper.Error("Validation Failed", HttpStatusCode.BadRequest, errors));
        }
        // var response = await _categoryClient.GetCategoryByIdAsync(new GetCategoryByIdRequest
        // {
        //     CategoryId = productDto.CategoryId
        // });
        // if (!response.IsFound)
        // {
        //     return BadRequest(ApiResponseHelper.Error("Category does not exist.", HttpStatusCode.BadRequest));
        // }
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
    public async Task<ActionResult<APIResponse>> GetProductsByCategory(int categoryId)
    {
        var response = await _categoryClient.GetCategoryByIdAsync(new GetCategoryByIdRequest
        {
            CategoryId = categoryId
        });

        if (!response.IsFound)
        {
            return BadRequest(ApiResponseHelper.Error("Category does not exist.", HttpStatusCode.BadRequest));
        }
        var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
        return Ok(ApiResponseHelper.Success(products, HttpStatusCode.OK));
    }
}
