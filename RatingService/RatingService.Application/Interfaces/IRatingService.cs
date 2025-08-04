using RatingService.Application.DTOs;
using RatingService.Domain.Models;

namespace RatingService.Application.Interfaces;

public interface IRatingService
{
    Task<RatingDto> AddRatingAsync(RatingDto ratingDto);
    Task<RatingDto?> GetRatingByProduct(int productId);
    Task<List<Rating>> GetCustomerRatings(int productId);
}
