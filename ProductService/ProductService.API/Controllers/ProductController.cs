using System.Net;
using Microservices.Shared;
using Microservices.Shared.Helpers;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Authorization;
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

    [AllowAnonymous]
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
    public async Task<ActionResult<APIResponse>> GetProductById(string id)
    {
        int? decodedId = id.DecodeToInt(HttpContext.RequestServices);
        if (decodedId == null)
            return NotFound(ApiResponseHelper.Error("Invalid Product ID", HttpStatusCode.NotFound));
        var product = await _productService.GetProductByIdAsync(decodedId.Value);
        if (product == null || product.Id == 0)
        {
            return NotFound(ApiResponseHelper.Error("Product Not Found", HttpStatusCode.NotFound));
        }

        return Ok(ApiResponseHelper.Success(product, HttpStatusCode.OK));
    }

    [HttpPost("Add")]
    [Authorize(Roles = Roles.Admin)]
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

        var response = await _categoryClient.GetCategoryByIdAsync(new GetCategoryByIdRequest
        {
            CategoryId = productDto.CategoryId
        });

        if (!response.IsFound)
        {
            return BadRequest(ApiResponseHelper.Error("Category does not exist.", HttpStatusCode.BadRequest));
        }
        var addedProduct = await _productService.AddProductAsync(productDto);
        return Ok(ApiResponseHelper.Success(addedProduct, HttpStatusCode.Created));
    }

    [HttpPut("Update")]
    [Authorize(Roles = Roles.Admin)]
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
        var response = await _categoryClient.GetCategoryByIdAsync(new GetCategoryByIdRequest
        {
            CategoryId = productDto.CategoryId
        });
        if (!response.IsFound)
        {
            return BadRequest(ApiResponseHelper.Error("Category does not exist.", HttpStatusCode.BadRequest));
        }
        var updatedProduct = await _productService.UpdateProductAsync(productDto);
        if (updatedProduct == null)
        {
            return NotFound(ApiResponseHelper.Error("Product not found", HttpStatusCode.NotFound));
        }

        return Ok(ApiResponseHelper.Success(updatedProduct, HttpStatusCode.OK));
    }

    [HttpPatch("Delete/{id}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        int? decodedId = id.DecodeToInt(HttpContext.RequestServices);
        if (decodedId == null)
            return NotFound(ApiResponseHelper.Error("Invalid Product ID", HttpStatusCode.NotFound));
        var isDeleted = await _productService.DeleteProductAsync(decodedId.Value);
        if (!isDeleted)
        {
            return NotFound(ApiResponseHelper.Error("Product not found", HttpStatusCode.NotFound));
        }

        return Ok(ApiResponseHelper.Success(HttpStatusCode.NoContent));
    }

    [HttpGet("GetByCategory/{hashedId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetProductsByCategory(string hashedId)
    {
        int? id = hashedId.DecodeToInt(HttpContext.RequestServices);

        if (id == null)
            return NotFound(ApiResponseHelper.Error("Invalid Product ID", HttpStatusCode.NotFound));

        var response = await _categoryClient.GetCategoryByIdAsync(new GetCategoryByIdRequest
        {
            CategoryId = id.Value
        });

        if (!response.IsFound)
        {
            return BadRequest(ApiResponseHelper.Error("Category does not exist.", HttpStatusCode.BadRequest));
        }
        var products = await _productService.GetProductsByCategoryIdAsync(id.Value);
        return Ok(ApiResponseHelper.Success(products, HttpStatusCode.OK));
    }

    [HttpGet("Search/{searchTerm}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetProductBySearch(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(ApiResponseHelper.Error("Search term cannot be empty.", HttpStatusCode.BadRequest));
        }
        var products = await _productService.GetProductBySearchAsync(searchTerm);
        if (products == null || products.Count == 0)
        {
            return NotFound(ApiResponseHelper.Error("No products found matching the search term.", HttpStatusCode.NotFound));
        }
        return Ok(ApiResponseHelper.Success(products, HttpStatusCode.OK));
    }
}
