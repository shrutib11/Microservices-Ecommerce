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
        Task<UserDto> CreateUser(UserDto model);
        public Task<List<UserDto>> GetAllUsers();
        public Task<UserDto?> GetUserById(int id);
        bool IsUserExist(string email);
        Task<UserDto?> UpdateUser(UserDto model);
    }
}