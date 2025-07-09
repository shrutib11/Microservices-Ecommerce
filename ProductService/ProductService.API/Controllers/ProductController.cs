using Microservices.Shared;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    protected APIResponse _response;
    public ProductController(IProductService productService)
    {
        _productService = productService;
        this._response = new APIResponse();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        _response.Result = products;
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        _response.IsSuccess = true;
        return Ok(_response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id) ?? throw new KeyNotFoundException("Product not found");
        _response.Result = product;
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        _response.IsSuccess = true;
        return Ok(_response);
    }
}
