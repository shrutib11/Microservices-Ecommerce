using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UserService.Domain.Models;

namespace UserService.Application.DTOs
{
    public class UserDto
    {
        // public List<User> Users { get; set; } = new List<User>();
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public IFormFile? UserFile { get; set; } 
        public string PinCode { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}