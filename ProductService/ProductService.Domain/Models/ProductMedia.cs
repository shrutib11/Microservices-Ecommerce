using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProductService.Domain.Enums;

namespace ProductService.Domain.Models;

public class ProductMedia
{
    [Key]
    public int Id { get; set; }
    public int ProductId { get; set; }
    public MediaType MediaType { get; set; }
    public string MediaUrl { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
}
