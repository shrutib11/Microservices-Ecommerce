using RatingService.Application.DTOs;

namespace RatingService.Application.Interfaces;

public interface IRatingService
{
    Task<RatingDto> AddRatingAsync(RatingDto ratingDto);
}
