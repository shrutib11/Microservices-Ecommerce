using System.Net;
using CategoryService.Application.Interfaces;
using Microservices.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CategoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    protected APIResponse _response;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        _response = new();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        _response.Result = categories;
        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        return Ok(_response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetCategoryById(id) ?? throw new KeyNotFoundException("Category Not Found");
        _response.Result = category;
        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        return Ok(_response);
    }

    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetCategoryById(id) ?? throw new KeyNotFoundException("Category Not Found");
        await _categoryService.Delete(id);
        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        return Ok(_response);
    }
}
