using Microsoft.EntityFrameworkCore;
using RatingService.Domain.Interfcaes;
using RatingService.Domain.Models;

namespace RatingService.Infrastructure.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly RatingServiceDbContext _context;

    public RatingRepository(RatingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Rating> AddAsync(Rating rating)
    {
        await _context.Ratings.AddAsync(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<int> GetTotalRatingsAsync(int productId)
    {
        return await _context.Ratings
                .Where(p => p.ProductId == productId).CountAsync();
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        return await _context.Ratings
            .Where(p => p.ProductId == productId).AverageAsync(p => p.RatingValue);
    }
}
