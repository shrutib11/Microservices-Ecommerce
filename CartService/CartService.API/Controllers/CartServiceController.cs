using System.Net;
using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using Microservices.Shared.Helpers;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartService.API.Controllers;

[Route("api/cart")]
[Authorize]
public class CartServiceController : ControllerBase
{
    private readonly ICartService _cartService;

    private readonly User.UserClient _userClient;

    private readonly Product.ProductClient _productClient;

    public CartServiceController(ICartService cartService, User.UserClient userClient, Product.ProductClient productClient)
    {
        _cartService = cartService;
        _userClient = userClient;
        _productClient = productClient;
    }

    [HttpPost("AddCart")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromForm] CartDto cartDto)
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

        var response = await _userClient.GetUserByIdAsync(new GetUserByIdRequest
        {
            UserId = cartDto.UserId
        });

        if (!response.IsFound)
        {
            return BadRequest(ApiResponseHelper.Error("User does not exist.", HttpStatusCode.BadRequest));
        }

        var cart = await _cartService.Add(cartDto);
        return Ok(ApiResponseHelper.Success(cart, HttpStatusCode.Created));
    }

    [HttpGet("by-user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserCart(int userId)
    {
        var response = await _userClient.GetUserByIdAsync(new GetUserByIdRequest
        {
            UserId = userId
        });

        if (!response.IsFound)
        {
            return BadRequest(ApiResponseHelper.Error("User does not exist.", HttpStatusCode.BadRequest));
        }

        CartDto? cart = await _cartService.GetUserCart(userId);
        if (cart is null || cart.Id == 0)
            return NotFound(ApiResponseHelper.Error("Cart Not Found", HttpStatusCode.NotFound));
        
        return Ok(ApiResponseHelper.Success(cart, HttpStatusCode.OK));
    }

    [HttpPatch("{id}/delete-item")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCartItem(string id)
    {
        int? decodedId = id.DecodeToInt(HttpContext.RequestServices);
        if (decodedId == null)
            return NotFound(ApiResponseHelper.Error("Invalid Product ID", HttpStatusCode.NotFound));
        var result = await _cartService.DeleteCartItem(decodedId.Value);
        if (result)
            return Ok(ApiResponseHelper.Success(null, HttpStatusCode.NoContent));
        else
            return NotFound(ApiResponseHelper.Error("CartItem Not Found", HttpStatusCode.NotFound));
    }

    [HttpPatch("{id}/clear-cart")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart(int id)
    {
        var result = await _cartService.ClearCart(id);
        if (result)
            return Ok(ApiResponseHelper.Success(null, HttpStatusCode.NoContent));
        else
            return NotFound(ApiResponseHelper.Error("Cart Not Found", HttpStatusCode.NotFound));
    }

    [HttpPatch("update-quantity")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateQuantity([FromForm] UpdateCartItemQuantityDto cartItemDto)
    {
        CartItemDto? dto = await _cartService.UpdateQuantity(cartItemDto);
        if (dto is null)
            return NotFound(ApiResponseHelper.Error("CartItem Not Found", HttpStatusCode.NotFound));
        else
            return Ok(ApiResponseHelper.Success(dto, HttpStatusCode.OK));
    }

    [HttpGet("by-id/{id}")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCartItemsByCartId(int id)
    {
        List<CartItemDto>? cartItems = await _cartService.GetCartItemsByCartId(id);
        if (cartItems is null)
            return NotFound(ApiResponseHelper.Error("Cart Not Found", HttpStatusCode.NotFound));
        
        var responseList = new List<CartItemResponseDto>();

        foreach (var item in cartItems)
        {
            var productResponse = await _productClient.GetProductByIdAsync(new ProductRequest
            {
                ProductId = item.ProductId
            });
            
            if (!productResponse.Product.IsFound)
            {
                return NotFound(ApiResponseHelper.Error("Product Not Found", HttpStatusCode.NotFound));
            }

            var productDto = new ProductDto
            {
                Name = productResponse.Product.Name,
                ImageUrl = productResponse.Product.Image
            };

            responseList.Add(new CartItemResponseDto
            {
                CartItem = item,
                Product = productDto
            });
        }
        return Ok(ApiResponseHelper.Success(responseList, HttpStatusCode.OK));
    }

    [HttpPost("AddToCart")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToCart([FromForm] CartItemDto cartItemDto)
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

        CartItemDto? dto = await _cartService.AddToCart(cartItemDto);
        if (dto is null)
            return NotFound(ApiResponseHelper.Error("Cart Not Found", HttpStatusCode.NotFound));
        else
            return Ok(ApiResponseHelper.Success(dto, HttpStatusCode.Created));
    }
}
