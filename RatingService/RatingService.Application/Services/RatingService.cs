using AutoMapper;
using RatingService.Application.DTOs;
using RatingService.Application.Interfaces;
using RatingService.Domain.Interfcaes;
using RatingService.Domain.Models;

namespace RatingService.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMapper _mapper;
    public RatingService(IRatingRepository ratingRepository, IMapper mapper)
    {
        _ratingRepository = ratingRepository;
        _mapper = mapper;
    }

    public async Task<RatingDto> AddRatingAsync(RatingDto ratingDto)
    {
        var rating = _mapper.Map<Rating>(ratingDto);
        await _ratingRepository.AddAsync(rating);

        var avg = await _ratingRepository.GetAverageRatingAsync(rating.ProductId);
        var count = await _ratingRepository.GetTotalRatingsAsync(rating.ProductId);

        ratingDto.AvgRating = Convert.ToDecimal(avg);
        ratingDto.TotalReviews = count;

        return ratingDto;
    }

    public async Task<RatingDto?> GetRatingByProduct(int productId)
    {
        var ratings = await _ratingRepository.GetRatingByProductAsync(productId);

        if (ratings == null || ratings.Count == 0)
            return null;

        var total = ratings.Count;
        var ratingCount = Enumerable.Range(1, 5).ToDictionary(i => i, i => 0);

        if (total > 0)
        {
            foreach (var group in ratings.GroupBy(r => r.RatingValue))
            {
                if (ratingCount.ContainsKey(group.Key))
                {
                    ratingCount[group.Key] = (int)Math.Round((group.Count() * 100.0) / total);
                }
            }
        }

        var average = ratings.Average(r => r.RatingValue);

        return new RatingDto
        {
            ProductId = productId,
            RatingDistribution = ratingCount,
            AvgRating = Convert.ToDecimal(average),
            TotalReviews = total
        };
    }

    public async Task<List<Rating>> GetCustomerRatings(int productId)
    {
        var ratings = await _ratingRepository.GetRatingByProductAsync(productId);
        return ratings;
    }
}

