using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Application.Services;

public class KeyclockService : IKeyclockService
{
    private readonly IConfiguration _configuration;
    private readonly string _clientId;
    private readonly string _clientSecret;

    private readonly IHttpClientFactory _httpClientFactory;

    public KeyclockService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;

        var microservicesClientConfig = _configuration.GetSection("Keycloack:Clients:MicroservicesClient");
        _clientId = microservicesClientConfig["ClientId"]!;
        _clientSecret = microservicesClientConfig["ClientSecret"]!;
    }

    public async Task<KeycloakTokenResponse?> GetTokens(string Username, string Password)
    {
        var tokenUrl = _configuration["Keycloack:Endpoints:Token"];
        var client = _httpClientFactory.CreateClient();

        var content = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", Username),
                new KeyValuePair<string, string>("password", Password)
            });

        var response = await client.PostAsync(tokenUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return new KeycloakTokenResponse
        {
            AccessToken = doc.RootElement.GetProperty("access_token").GetString()!,
            RefreshToken = doc.RootElement.GetProperty("refresh_token").GetString()!,
            ExpiresIn = doc.RootElement.GetProperty("expires_in").GetInt32(),
            RefreshExpiresIn = doc.RootElement.GetProperty("refresh_expires_in").GetInt32()
        };
    }

    public async Task<string> CreateUserInKeycloak(string adminAccessToken, UserDto userModel)
    {
        var userUrl = _configuration["Keycloack:Endpoints:User"];
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

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
                        value = userModel.Password,
                        temporary = false
                    }
                 }
        };

        var content = new StringContent(JsonConvert.SerializeObject(userPayload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(userUrl, content);

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

    public async Task<bool> AssignRealmRolesToUser(string userId, string adminAccessToken, List<string> roleNames)
    {
        var roleMappingTemplate = _configuration["Keycloack:Endpoints:RoleMappings"]!;
        var rolesTemplate = _configuration["Keycloack:Endpoints:Roles"]!;
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        var roleMappingUrl = roleMappingTemplate.Replace("{userId}", userId);

        var rolesToAssign = roleNames.Select(roleName => new
        {
            id = "",
            name = roleName
        }).ToList();

        var detailedRoles = new List<object>();
        foreach (var roleName in roleNames)
        {
            var roleUrl = rolesTemplate.Replace("{roleName}", roleName);

            var roleResponse = await client.GetAsync(roleUrl);
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

        var response = await client.PostAsJsonAsync(roleMappingUrl, detailedRoles);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPassword(string adminAccessToken, string userId, string newPassword)
    {
        var resetPasswordTemplate = _configuration["Keycloack:Endpoints:ResetPassword"]!;
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        var resetPasswordUrl = resetPasswordTemplate.Replace("{userId}", userId);

        var passwordPayload = new
        {
            type = "password",
            value = newPassword,
            temporary = false
        };

        var response = await client.PutAsJsonAsync(resetPasswordUrl, passwordPayload);

        return response.IsSuccessStatusCode;
    }


    public async Task<KeycloakTokenResponse?> RefreshTokens(string refreshToken)
    {
        var tokenUrl = _configuration["Keycloack:Endpoints:Token"];

        var parameters = new Dictionary<string, string>
    {
        { "client_id", _clientId },
        { "client_secret", _clientSecret },
        { "grant_type", "refresh_token" },
        { "refresh_token", refreshToken }
    };

        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(parameters));
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return new KeycloakTokenResponse
        {
            AccessToken = doc.RootElement.GetProperty("access_token").GetString()!,
            RefreshToken = doc.RootElement.GetProperty("refresh_token").GetString()!,
            ExpiresIn = doc.RootElement.GetProperty("expires_in").GetInt32(),
            RefreshExpiresIn = doc.RootElement.GetProperty("refresh_expires_in").GetInt32()
        };
    }

    public async Task<bool> Logout(string refreshToken)
    {
        if (!await IsTokenActive(refreshToken))
            return false;

        var logoutUrl = _configuration["Keycloack:Endpoints:Logout"];
        var client = _httpClientFactory.CreateClient();

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "refresh_token", refreshToken }
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await client.PostAsync(logoutUrl, content);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> IsTokenActive(string refreshToken)
    {
        var introspectionUrl = _configuration["Keycloack:Endpoints:Introspect"];
        var client = _httpClientFactory.CreateClient();

        var parameters = new Dictionary<string, string>
    {
        { "client_id", _clientId },
        { "client_secret", _clientSecret },
        { "token", refreshToken }
    };

        var content = new FormUrlEncodedContent(parameters);
        var response = await client.PostAsync(introspectionUrl, content);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.TryGetProperty("active", out var active) && active.GetBoolean();
    }



}
