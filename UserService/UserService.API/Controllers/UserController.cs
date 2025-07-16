
using System.Net;
using AutoMapper;
using Microservices.Shared;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Models;


namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/User")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet("GetAll", Name = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(ApiResponseHelper.Success(await _userService.GetAllUsers(), HttpStatusCode.OK));
        }

        [HttpGet("GetById/{id}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetId(int id)
        {
            UserDto? model = await _userService.GetUserById(id);
            if (model == null || model.Id == 0)
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }
            return Ok(ApiResponseHelper.Success(model, HttpStatusCode.OK));
        }

        [HttpPost("Register", Name = "RegisterUser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromForm] UserDto model)
        {
            if (_userService.IsUserExist(model.Email))
            {
                return Conflict(ApiResponseHelper.Error("User with this email already exists.", HttpStatusCode.Conflict));
            }
            UserDto userModel = await _userService.CreateUser(model);
            return CreatedAtRoute("GetUserById", new { id = userModel.Id }, ApiResponseHelper.Success(userModel, HttpStatusCode.Created));
        }

        [HttpPut("Update", Name = "UpdateUser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromForm] UserDto model)
        {
            Console.WriteLine("hiii");
            UserDto? userModel = await _userService.UpdateUser(model);
            if (userModel == null)
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }
            return Ok(ApiResponseHelper.Success(userModel));
        }

        [HttpPatch("TestPatch/{id}", Name = "TestPatchUser")]
        public async Task<IActionResult> TestPatch(int id, [FromBody] JsonPatchDocument<User> PatchDoc)
        {
            if (PatchDoc == null)
            {
                return BadRequest(ApiResponseHelper.Error("Patch document cannot be null", HttpStatusCode.BadRequest));
            }
            
            UserDto? userDto = await _userService.GetUserById(id);
            User user = _mapper.Map<User>(userDto);

            if (user == null)
            {
                return NotFound(ApiResponseHelper.Error("User not found", HttpStatusCode.NotFound));
            }

            PatchDoc.ApplyTo(user, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseHelper.Error("Invalid patch operation", HttpStatusCode.BadRequest));
            }
            return Ok(ApiResponseHelper.Success(user, HttpStatusCode.OK));
        }
    }
}