using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Interfaces;
using UserService.Domain.Models;

namespace UserService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository,IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            List<User> users = await _userRepository.GetAllUsers();
            return  _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserById(int id)
        {
            User user = await _userRepository.GetUserByIdSysnc(id) ?? new User();
            if (user.Id == 0)
            {
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }
    }
}