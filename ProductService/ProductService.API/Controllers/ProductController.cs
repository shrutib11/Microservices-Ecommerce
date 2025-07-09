using System.Net;
using Microservices.Shared;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
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
        var product = await _productService.GetProductByIdAsync(id)
                    ?? throw new HttpStatusCodeException($"Product with ID {id} not found.", HttpStatusCode.NotFound);

        return Ok(ApiResponseHelper.Success(product, HttpStatusCode.OK));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProduct([FromForm] ProductDto productDto)
    {
        var addedProduct = await _productService.AddProductAsync(productDto);
        return CreatedAtAction(nameof(GetProductById), new { id = addedProduct.Id },
            ApiResponseHelper.Success(addedProduct, HttpStatusCode.Created));
    }

    [HttpPut]
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

    [HttpDelete("{id}")]
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


    

}
