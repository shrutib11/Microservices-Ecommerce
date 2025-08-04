using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Domain.Models;

namespace UserService.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> CreateUser(User newUser);
        public Task<List<User>> GetAllUsers();
        public Task<User> GetUserByEmailAysnc(string email);
        public Task<User?> GetUserByIdSysnc(int id);
        public Task<User> UpdateUserAsync(User user);
        public Task<List<User>> GetByIdsAsync(List<int> userIds);
    }
}