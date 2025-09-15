using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(6)]
        [MinLength(6)]
        public string PinCode { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? ProfileImage { get; set; }

        public string? Role { get; set; }

        [Required]
        [MaxLength(36)]
        public string KeycloackId { get; set; } = string.Empty;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}