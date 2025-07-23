using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public IFormFile? UserFile { get; set; } 
        public string PinCode { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
    }
}