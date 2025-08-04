using RatingService.Domain.Models;

namespace RatingService.Domain.Interfcaes;

public interface IRatingRepository
{
    Task<Rating> AddAsync(Rating rating);
    Task<int> GetTotalRatingsAsync(int productId);
    Task<double> GetAverageRatingAsync(int productId);
    Task<List<Rating>> GetRatingByProductAsync(int productId);
}
