using AutoMapper;
using Microservices.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _imageHelper = new ImageHelper();
        }

        public async Task<UserDto> CreateUser(UserDto model)
        {
            model.Password = PasswordHelper.HashPassword(model.Password);
            User newUser = _mapper.Map<User>(model);
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (model.UserFile != null)
                newUser.ProfileImage = ImageHelper.SaveImageWithName(model.UserFile, model.FirstName, rootPath);
            else
                newUser.ProfileImage = "uploads/default.png";
            newUser.CreatedAt = DateTime.Now;
            return await _userRepository.CreateUser(newUser) != null ? _mapper.Map<UserDto>(newUser) : throw new Exception("Failed to create user.");
        }

        public async Task<UserDto> CreateUser(UserDto model)
        {
            model.Password = PasswordHelper.HashPassword(model.Password);
            User newUser = _mapper.Map<User>(model);
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (model.UserFile != null)
                newUser.ProfileImage = ImageHelper.SaveImageWithName(model.UserFile, model.FirstName, rootPath);
            else
                newUser.ProfileImage = "/uploads/default.png";
            newUser.CreatedAt = DateTime.Now;
            return await _userRepository.CreateUser(newUser) != null ? _mapper.Map<UserDto>(newUser) : throw new Exception("Failed to create user.");
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            List<User> users = await _userRepository.GetAllUsers();
            return _mapper.Map<List<UserDto>>(users);
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

        public bool IsUserExist(string email)
        {
            User user = _userRepository.GetUserByEmailAysnc(email).Result;
            return user.Id != 0;
        }

        public async Task<UserDto?> UpdateUser(UserDto model)
        {
            User? currentUser = await _userRepository.GetUserByIdSysnc(model.Id);
            if (model.Password != null)
            {
                model.Password = PasswordHelper.HashPassword(model.Password);
            }
            if (currentUser == null)
            {
                return null;
            }
            _mapper.Map(model, currentUser);
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (model.UserFile == null)
            {
                currentUser.ProfileImage = "/uploads/default.png";
            }
            else
            {
                currentUser.ProfileImage = ImageHelper.SaveImageWithName(model.UserFile, model.FirstName, rootPath);
            }
            currentUser.UpdatedAt = DateTime.Now;

            UserDto userModel = _mapper.Map<UserDto>(await _userRepository.UpdateUserAsync(currentUser));
            return userModel ?? throw new Exception("Failed to update user.");
        }
    }
}