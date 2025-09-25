using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IKeyclockService
{
    public Task<KeycloakTokenResponse?> GetTokens(string Username, string Password);
    public Task<string> CreateUserInKeycloak(string adminAccessToken, UserDto userModel);
    public Task<bool> AssignRealmRolesToUser(string userId, string adminAccessToken, List<string> roleNames);
    public Task<bool> ResetPassword(string adminAccessToken, string userId, string newPassword);
    public Task<KeycloakTokenResponse?> RefreshTokens(string refreshToken);
    public Task<bool> Logout(string refreshToken);
}
