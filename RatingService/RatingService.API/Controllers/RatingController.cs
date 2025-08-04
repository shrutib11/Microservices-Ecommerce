using System.Net;
using Microservices.Shared.Helpers;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingService.Application.DTOs;
using RatingService.Application.Interfaces;
using GrpcUser = Microservices.Shared.Protos.User;

namespace RatingService.API.Controllers;

// [ApiController]
[Authorize]
[Route("api/Ratings")]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly Product.ProductClient _productClient;
    private readonly GrpcUser.UserClient  _userClient;
    public RatingController(IRatingService ratingService, Product.ProductClient productClient, GrpcUser.UserClient userClient)
    {
        _productClient = productClient;
        _ratingService = ratingService;
        _userClient = userClient;
    }

    [HttpPost("Add")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRating([FromForm] RatingDto ratingDto)
    {
        var userResponse = await _userClient.GetUserByIdAsync(new GetUserByIdRequest
        {
            UserId = ratingDto.UserId
        });

        if (!userResponse.IsFound)
        {
            return NotFound(ApiResponseHelper.Error("User not found with this id.", HttpStatusCode.NotFound));
        }
        
        var productResponse = await _productClient.GetProductByIdAsync(new ProductRequest
        {
            ProductId = ratingDto.ProductId
        });

        if (!productResponse.Product.IsFound)
        {
            return NotFound(ApiResponseHelper.Error("Product not found with this id.", HttpStatusCode.NotFound));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(ms => ms.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                );
            return BadRequest(new { Status = "Error", Message = "Validation Failed", Errors = errors });
        }

        var addedRating = await _ratingService.AddRatingAsync(ratingDto);

        var productRatingResponse = await _productClient.UpdateRatingInfoAsync(new UpdateProductRatingsRequest
        {
            ProductId = ratingDto.ProductId,
            AvgRating = (double)(addedRating.AvgRating ?? 0.0m),
            TotalRatings = addedRating.TotalReviews ?? 0
        });

        if (!productRatingResponse.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ApiResponseHelper.Error("Failed to update product ratings.", HttpStatusCode.InternalServerError));
        }

        return Ok(ApiResponseHelper.Success(addedRating, HttpStatusCode.Created));
    }

    [HttpGet("GetByProduct/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRatingByProduct(int productId)
    {
        var productResponse = await _productClient.GetProductByIdAsync(new ProductRequest
        {
            ProductId = productId
        });

        if (!productResponse.Product.IsFound)
        {
            return NotFound(ApiResponseHelper.Error("Product not found.", HttpStatusCode.NotFound));
        }
        var rating = await _ratingService.GetRatingByProduct(productId);
        if (rating == null)
        {
            return NotFound(ApiResponseHelper.Error("Rating not found for the specified product.", HttpStatusCode.NotFound));
        }

        return Ok(ApiResponseHelper.Success(rating, HttpStatusCode.OK));
    }

    [HttpGet("GetCustomerRatings/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerRatings(int productId)
    {
        var productResponse = await _productClient.GetProductByIdAsync(new ProductRequest
        {
            ProductId = productId
        });

        if (!productResponse.Product.IsFound)
        {
            return NotFound(ApiResponseHelper.Error("Product not found.", HttpStatusCode.NotFound));
        }

        var ratings = await _ratingService.GetCustomerRatings(productId);
        if (ratings.Count == 0)
        {
            return NotFound(ApiResponseHelper.Error("No ratings found for the specified product.", HttpStatusCode.NotFound));
        }

        var uniqueUserIds = ratings.Select(r => r.UserId).Distinct().ToList();
        var grpcResponse = await _userClient.GetUsersByIdsAsync(new GetUsersByIdsRequest { UserIds = { uniqueUserIds } });

        var usermap = grpcResponse.Users.ToDictionary(u => u.UserId, u => u);

        var result = ratings.Select(r =>
        {
            var user = usermap.GetValueOrDefault(r.UserId);

            return new RatingDto
            {
                ProductId = r.ProductId,
                UserId = r.UserId,
                OrderId = r.OrderId,
                RatingValue = r.RatingValue,
                Comment = r.Comment,
                ReviewDate = r.CreatedAt,
                ReviewerName = user?.UserName ?? "Unknown",
                UserProfile = user?.UserImage ?? string.Empty,
            };
        }).ToList();

        return Ok(ApiResponseHelper.Success(result, HttpStatusCode.OK));
    }
}
