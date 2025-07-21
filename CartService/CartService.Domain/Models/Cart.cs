using System.ComponentModel.DataAnnotations;

namespace CartService.Domain.Models;

public class Cart
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}
