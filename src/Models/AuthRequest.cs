namespace JwtRoleAuthentication.Models;

public class AuthRequest
{
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
}