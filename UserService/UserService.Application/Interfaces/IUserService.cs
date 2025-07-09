using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Domain.Models;

namespace UserService.Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<UserDto>> GetAllUsers();
        public Task<UserDto?> GetUserById(int id);
    }
}