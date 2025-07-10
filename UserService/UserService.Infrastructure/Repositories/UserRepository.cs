using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Interfaces;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserServiceDbContext _context;
        public UserRepository(UserServiceDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUser(User newUser)
        {
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser ?? throw new Exception("User creation failed.");
        }


        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync() ?? throw new Exception("An error occurred while retrieving users.");
        }

        public async Task<User> GetUserByEmailAysnc(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email == email)  ?? new User();
        }

        public async Task<User?> GetUserByIdSysnc(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Update(user);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                return user;
            throw new Exception("User update failed.");
        }
    }
}