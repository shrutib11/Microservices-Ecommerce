using System.Net;
using System.Threading.Tasks;
using Microservices.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingService.Application.DTOs;
using RatingService.Application.Interfaces;

namespace RatingService.API.Controllers;

// [ApiController]
[Authorize]
[Route("api/Ratings")]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;
    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }


    [HttpPost("Add")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRating([FromForm] RatingDto ratingDto)
    {
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
        return Ok(ApiResponseHelper.Success(addedRating, HttpStatusCode.Created));
    }
}
