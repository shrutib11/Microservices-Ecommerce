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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKeyclockService _keyclockService;
        public UserController(IUserService userService, IConfiguration configuration, IMapper mapper, IJwtService jwtService, IHttpClientFactory httpClientFactory, IKeyclockService keyclockService)
        {
            _userService = userService;
            _mapper = mapper;
            _configuration = configuration;
            _jwtService = jwtService;
            _httpClientFactory = httpClientFactory;
            _keyclockService = keyclockService;
        }

        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        [HttpPost("Login", Name = "LoginUser")]
        public async Task<IActionResult> Login([FromForm] LoginDto loginData)
        {
            var Token = await _keyclockService.GetTokens(loginData.email, loginData.password);
            if (Token?.AccessToken == null)
            {
                return Unauthorized(ApiResponseHelper.Error("Invalid credentials", HttpStatusCode.Unauthorized));
            }

            UserDto? usermodel = await _userService.GetUserByEmail(loginData.email);
            if (usermodel == null || usermodel.Id == 0)
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }

            var result = new
            {
                user = usermodel,
                Tokens = Token
            };

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
        public async Task<IActionResult> SendEmail(EmailRequestModel model)
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
            else
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }

            await _userService.UpdateUser(currentUser!);

            var Username = _configuration["Keycloack:Username"];
            var Password = _configuration["Keycloack:Password"];
            var Token = await _keyclockService.GetTokens(Username!, Password!);

            if (Token?.AccessToken == null)
            {
                return Unauthorized(ApiResponseHelper.Error("Invalid credentials", HttpStatusCode.Unauthorized));
            }

            var keycloakUserId = await _userService.GetKeycloackIdByEmail(currentUser.Email);

            if (string.IsNullOrEmpty(keycloakUserId))
            {
                return NotFound(ApiResponseHelper.Error("Keycloak user not found.", HttpStatusCode.NotFound));
            }

            var result = await _keyclockService.ResetPassword(Token!.AccessToken,keycloakUserId, newPassword!);

            if (!result)
            {
                return BadRequest(ApiResponseHelper.Error("Failed to reset password in Keycloak.", HttpStatusCode.BadRequest));
            }

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

            var Username = _configuration["Keycloack:Username"];
            var Password = _configuration["Keycloack:Password"];

            var Token = await _keyclockService.GetTokens(Username!, Password!);

            if (Token?.AccessToken == null)
            {
                return Unauthorized(ApiResponseHelper.Error("Invalid credentials", HttpStatusCode.Unauthorized));
            }

            string keycloakUserId = await _keyclockService.CreateUserInKeycloak(Token!.AccessToken, model);

            if (string.IsNullOrEmpty(keycloakUserId))
            {
                return BadRequest(ApiResponseHelper.Error("Failed to create user in Keycloak.", HttpStatusCode.BadRequest));
            }

            var rolesToAssign = new List<string> { "User" };
            var rolesAssigned = await _keyclockService.AssignRealmRolesToUser(keycloakUserId, Token!.AccessToken, rolesToAssign);

            if (!rolesAssigned)
            {
                return BadRequest(ApiResponseHelper.Error("Failed to assign roles to Keycloak user.", HttpStatusCode.BadRequest));
            }

            UserDto userModel = await _userService.CreateUser(model, keycloakUserId);

            var responseModel = new
            {
                user = userModel,
                Tokens = Token
            };

            return CreatedAtRoute("GetUserById", new { id = userModel.Id }, ApiResponseHelper.Success(responseModel, HttpStatusCode.Created));

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

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(ApiResponseHelper.Error("Refresh token is expired.", HttpStatusCode.Unauthorized));
            }

            var isLoggedOut = await _keyclockService.Logout(refreshToken);
            if (isLoggedOut)
            {
                return Ok(ApiResponseHelper.Success("Logged out successfully.", HttpStatusCode.OK));
            }
            else
            {
                return Unauthorized(ApiResponseHelper.Error("token already expired.", HttpStatusCode.Unauthorized));
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(KeycloakTokenResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetToken([FromBody] string refershToken)
        {
           var Tokens = await _keyclockService.RefreshTokens(refershToken);

            if (Tokens?.AccessToken == null)
            {
                return Unauthorized(ApiResponseHelper.Error("Invalid refershToken", HttpStatusCode.Unauthorized));
            }

            return Ok(ApiResponseHelper.Success(Tokens, HttpStatusCode.OK));
        }
    }
}
public class EmailRequestModel
{
    public string Email { get; set; } = string.Empty;
}