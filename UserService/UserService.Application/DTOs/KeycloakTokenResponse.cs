namespace UserService.Application.DTOs;

public class KeycloakTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public int RefreshExpiresIn { get; set; }
}
