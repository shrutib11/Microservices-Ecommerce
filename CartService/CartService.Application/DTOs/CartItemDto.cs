namespace CartService.Application.DTOs;

public class CartItemDto
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? PriceAtAddTime { get; set; }
}
