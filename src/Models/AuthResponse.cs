namespace JwtRoleAuthentication.Models;

public class AuthResponse
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Token { get; set; }
}