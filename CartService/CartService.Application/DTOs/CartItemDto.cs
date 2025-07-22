namespace CartService.Application.DTOs;

public class CartItemDto
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? PriceAtAddTime { get; set; }
}

public class CartItemResponseDto
{
    public CartItemDto CartItem { get; set; } = default!;

    public ProductDto Product { get; set; } = default!;
}

public class ProductDto
{
    public string Name { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;
}

public class UpdateCartItemQuantityDto
{
    public int Id { get; set; }
    
    public decimal? Quantity { get; set; }
}
