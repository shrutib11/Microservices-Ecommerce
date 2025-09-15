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
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;

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
        public UserController(IUserService userService, IConfiguration configuration, IMapper mapper, IJwtService jwtService, IHttpClientFactory httpClientFactory)
        {
            _userService = userService;
            _mapper = mapper;
            _configuration = configuration;
            _jwtService = jwtService;
            _httpClientFactory = httpClientFactory;
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
            var result = new { token, user };
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

            var microservicesClientConfig = _configuration.GetSection("Jwt:Clients:MicroservicesClient");
            var clientId = microservicesClientConfig["ClientId"];
            var clientSecret = microservicesClientConfig["ClientSecret"];
            var authority = _configuration["Jwt:Authority"];

            var accessToken = await GetAccessToken(authority!, clientId!, clientSecret!);





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

            // UserDto userModel = await _userService.CreateUser(model);

            var microservicesClientConfig = _configuration.GetSection("Jwt:Clients:MicroservicesClient");
            var clientId = microservicesClientConfig["ClientId"];
            var clientSecret = microservicesClientConfig["ClientSecret"];
            var authority = _configuration["Jwt:Authority"];

            var accessToken = await GetAccessToken(authority!, clientId!, clientSecret!);

            string keycloakUserId = await CreateUserInKeycloak(accessToken, model, model.Password);

            var rolesToAssign = new List<string> { "User" };
            bool rolesAssigned = await AssignRealmRolesToUser(keycloakUserId, "my-realm", accessToken, rolesToAssign);

            if (!rolesAssigned)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponseHelper.Error("Failed to assign roles to Keycloak user.", HttpStatusCode.InternalServerError));
            }

            model.KeycloackId = keycloakUserId;
            UserDto userModel = await _userService.CreateUser(model);

            var responseModel = new
            {
                user = userModel,
                Token = accessToken
            };

            return CreatedAtRoute("GetUserById", new { id = userModel.Id }, ApiResponseHelper.Success(responseModel, HttpStatusCode.Created));

            // return CreatedAtRoute("GetUserById", new { id = userModel.Id }, ApiResponseHelper.Success(userModel, HttpStatusCode.Created));
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
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(ApiResponseHelper.Error("Refresh token is expired.", HttpStatusCode.Unauthorized));
            }

            var authority = _configuration["Jwt:Authority"];
            // var clientId = _configuration["Jwt:ClientId"];
            var angularClientConfig = _configuration.GetSection("Jwt:Clients:AngularClient");
            var angularClientId = angularClientConfig["ClientId"];

            var client = _httpClientFactory.CreateClient();

            var formData = new Dictionary<string, string>
            {
                { "client_id", angularClientId! },
                { "refresh_token", refreshToken }
            };

            var response = await client.PostAsync($"{authority}/protocol/openid-connect/logout", new FormUrlEncodedContent(formData));

            if (response.IsSuccessStatusCode)
            {
                return Ok(ApiResponseHelper.Success("Logged out successfully.", HttpStatusCode.OK));
            }
            else
            {
                return Unauthorized(ApiResponseHelper.Error("Failed to log out.", HttpStatusCode.Unauthorized));
            }
        }

        private async Task<string> GetAccessToken(string authority, string clientId, string clientSecret)
        {
            var tokenEndpoint = $"{authority}/protocol/openid-connect/token";
            var Username = _configuration["Keycloack:Username"];
            var Password = _configuration["Keycloack:Password"];
            var client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", Username!),
                new KeyValuePair<string, string>("password", Password!)
            });

            var response = await client.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to retrieve access token from Keycloak");

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<JObject>(responseContent);

            return tokenData?["access_token"]?.ToString() ?? throw new Exception("Access token not found in response");
        }

        private async Task<string> CreateUserInKeycloak(string accessToken, UserDto userModel, string password)
        {
            var keycloackPort = _configuration["Keycloack:Port"];
            var keycloakEndpoint = $"{keycloackPort}/admin/realms/my-realm/users";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var userPayload = new
            {
                username = userModel.Email,
                firstName = userModel.FirstName,
                lastName = userModel.LastName,
                email = userModel.Email,
                enabled = true,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                 }
            };

            var content = new StringContent(JsonConvert.SerializeObject(userPayload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(keycloakEndpoint, content);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                if (response.Headers.Location != null)
                {
                    var locationUrl = response.Headers.Location.ToString();
                    var userId = locationUrl.Substring(locationUrl.LastIndexOf('/') + 1);
                    return userId;
                }
                else
                {
                    throw new Exception("User created but no location header found.");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create user in Keycloak: {response.StatusCode} - {errorContent}");
            }
        }

        private async Task<bool> AssignRealmRolesToUser(string userId, string realm, string accessToken, List<string> roleNames)
        {
            var keycloackPort = _configuration["Keycloack:Port"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var rolesToAssign = roleNames.Select(roleName => new
            {
                id = "",
                name = roleName
            }).ToList();

            var detailedRoles = new List<object>();
            foreach (var roleName in roleNames)
            {
                var roleResponse = await client.GetAsync($"{keycloackPort}/admin/realms/{realm}/roles/{roleName}");
                if (!roleResponse.IsSuccessStatusCode)
                {
                    return false;
                }
                var roleContent = await roleResponse.Content.ReadFromJsonAsync<JsonElement>();
                detailedRoles.Add(new
                {
                    id = roleContent.GetProperty("id").GetString(),
                    name = roleContent.GetProperty("name").GetString()
                });
            }

            var response = await client.PostAsJsonAsync($"{keycloackPort}/admin/realms/{realm}/users/{userId}/role-mappings/realm", detailedRoles);

            return response.IsSuccessStatusCode;
        }

        [HttpGet("GetUserIdFromKeycloak/{keycloakId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserIdFromKeycloak(string keycloakId)
        {
            int userId = await _userService.GetUserIdByKeycloackId(keycloakId);
            if (userId == 0)
            {
                return NotFound(ApiResponseHelper.Error("User Not Found", HttpStatusCode.NotFound));
            }
            return Ok(ApiResponseHelper.Success(userId, HttpStatusCode.OK));
        }
    }
}
public class EmailRequestModel
{
    public string Email { get; set; } = string.Empty;
}