using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryService.Domain.Models;

public class Category
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(20)]
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    [MaxLength(200)]
    public string CategoryImage { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}
