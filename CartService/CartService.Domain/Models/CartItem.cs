using System.ComponentModel.DataAnnotations;

namespace CartService.Domain.Models;

public class CartItem
{
    [Key]
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal? PriceAtAddTime { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}
