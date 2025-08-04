namespace RatingService.Application.DTOs;

public class RatingDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int RatingValue { get; set; }
    public string? Comment { get; set; }
    public decimal? AvgRating { get; set; }
    public int? TotalReviews { get; set; } 
    public Dictionary<int, int>? RatingDistribution { get; set; } 
    public string? ReviewerName { get; set; }             
    public DateTime? ReviewDate { get; set; }            
    public string? UserProfile { get; set; }
}
