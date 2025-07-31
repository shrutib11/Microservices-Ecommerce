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

        var avg  = await _ratingRepository.GetAverageRatingAsync(rating.ProductId);
        var count = await _ratingRepository.GetTotalRatingsAsync(rating.ProductId);

        ratingDto.AvgRating = Convert.ToDecimal(avg);
        ratingDto.TotalReviews = count;

        return ratingDto;
    }
}

