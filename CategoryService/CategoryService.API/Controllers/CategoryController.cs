using System.Net;
using CategoryService.Application.DTOs;
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

    [HttpGet("GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(ApiResponseHelper.Success(categories, HttpStatusCode.OK));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetCategoryById(id) ;
        if (category == null || category.Id == 0)
        { 
            return NotFound(ApiResponseHelper.Error("Category Not Found", HttpStatusCode.NotFound));
        }
        return Ok(ApiResponseHelper.Success(category, HttpStatusCode.OK));
    }

    [HttpPost("Add")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromForm] CategoryDto categoryDto)
    {
        var category = await _categoryService.Add(categoryDto);
        return Ok(ApiResponseHelper.Success(category, HttpStatusCode.Created));
    }

    [HttpPut("Update")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromForm] CategoryDto categoryDto)
    {
        var category = await _categoryService.GetCategoryById(categoryDto.Id);
        if (category == null)
        {
            return NotFound(ApiResponseHelper.Error("Category Not Found", HttpStatusCode.NotFound));
        }
        category = await _categoryService.Update(categoryDto);
        return Ok(ApiResponseHelper.Success(category, HttpStatusCode.OK));
    }

    [HttpPatch("Delete/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetCategoryById(id);
        if (category == null)
        {
            return NotFound(ApiResponseHelper.Error("Category Not Found", HttpStatusCode.NotFound));
        }
        await _categoryService.Delete(id);
        return Ok(ApiResponseHelper.Success(null, HttpStatusCode.NoContent));
    }
}
