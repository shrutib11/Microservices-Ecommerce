using System.ComponentModel.DataAnnotations;

namespace SupportService.Domain.Models;

public class FAQs
{
    [Key]
    public int Id { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public List<string> Keyword { get; set; } = new();
}
