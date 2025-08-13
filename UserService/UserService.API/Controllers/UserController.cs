using System.Net;
using AutoMapper;
using Microservices.Shared;
using Microservices.Shared.Helpers;
using Microservices.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using System.Security.Claims;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/User")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private IConfiguration _configuration;
        public UserController(IUserService userService, IConfiguration configuration, IMapper mapper, IJwtService jwtService)
        {
            _userService = userService;
            _mapper = mapper;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        [HttpPost("Login", Name = "LoginUser")]
        public async Task<IActionResult> Login([FromForm] LoginDto loginData)
        {
            UserDto? user = await _userService.GetUserByEmail(loginData.email);
            if (user == null || user.Id == 0)
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }
            if (!PasswordHelper.VerifyPassword(loginData.password, user.Password))
            {
                return Unauthorized(ApiResponseHelper.Error("Invalid Password", HttpStatusCode.Unauthorized));
            }
            string token = _jwtService.generateJwtToken(user.Email, user.Role!, user.Id);
            var result = new{ token , user };
            return Ok(ApiResponseHelper.Success(result, HttpStatusCode.OK));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll", Name = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(ApiResponseHelper.Success(await _userService.GetAllUsers(), HttpStatusCode.OK));
        }

        [HttpPost("sendEmail")]
        public async Task<IActionResult> SendEmail( EmailRequestModel model)
        {
            UserDto? isUserExist = await _userService.GetUserByEmail(model.Email);
            if (isUserExist == null)
            {
                return NotFound(ApiResponseHelper.Error("User not Exist !! Register First !! ", HttpStatusCode.NotFound));
            }
            string link = "http://localHost:4200/user/reset-password/" + HashidsHelper.EncodeEmail(model.Email);
            await EmailHelper.SendEmailAsync(model.Email, "Reset password", new EmailTemplateViewModel()
            {
                ResetPasswordUrl = link,
                Name = isUserExist.FirstName + " " + isUserExist.FirstName
            }, _configuration);
            return Ok();
        }

        [HttpPut("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] JObject userData)
        {
            string? email = userData["email"]?.ToString();
            string? newPassword = userData["newPassword"]?.ToString();
            UserDto? currentUser = await _userService.GetUserByEmail(email!.DecodeEmail()!);
            if (currentUser != null)
            {
                currentUser.Password = PasswordHelper.HashPassword(newPassword!);
            }
            await _userService.UpdateUser(currentUser!);
            return Ok(ApiResponseHelper.Success(currentUser, HttpStatusCode.OK));
        }

        [AllowAnonymous]
        [HttpGet("GetById/{id}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetId(string id)
        {
            int? decodedId = id.DecodeToInt(HttpContext.RequestServices);
            if (decodedId == null)
                return NotFound(ApiResponseHelper.Error("Invalid Product ID", HttpStatusCode.NotFound));
            UserDto? model = await _userService.GetUserById(decodedId.Value);
            if (model == null || model.Id == 0)
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }
            return Ok(ApiResponseHelper.Success(model, HttpStatusCode.OK));
        }

        [AllowAnonymous]
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

        [Authorize(Roles = "Admin,User")]
        [HttpPut("Update", Name = "UpdateUser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromForm] UserDto model)
        {
            UserDto? userModel = await _userService.UpdateUser(model);
            if (userModel == null)
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }
            return Ok(ApiResponseHelper.Success(userModel));
        }

        // [HttpPatch("TestPatch/{id}", Name = "TestPatchUser")]
        // public async Task<IActionResult> TestPatch(int id, [FromBody] JsonPatchDocument<User> PatchDoc)
        // {
        //     if (PatchDoc == null)
        //     {
        //         return BadRequest(ApiResponseHelper.Error("Patch document cannot be null", HttpStatusCode.BadRequest));
        //     }
        //     UserDto? userDto = await _userService.GetUserById(id);
        //     User user = _mapper.Map<User>(userDto);

        //     if (user == null)
        //     {
        //         return NotFound(ApiResponseHelper.Error("User not found", HttpStatusCode.NotFound));
        //     }

        //     PatchDoc.ApplyTo(user, ModelState);

        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ApiResponseHelper.Error("Invalid patch operation", HttpStatusCode.BadRequest));
        //     }
        //     return Ok(ApiResponseHelper.Success(user, HttpStatusCode.OK));
        // }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            var useremail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(useremail))
            {
                return Unauthorized(ApiResponseHelper.Error("User not authenticated.", HttpStatusCode.Unauthorized));
            }

            return Ok("Logged out successfully.");
        }
    }
}
public class EmailRequestModel
{
    public string Email { get; set; } = string.Empty;
}