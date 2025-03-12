namespace BloggingPlatformApi.Models;

public class LoginModel
{
    public required string Username { get; set; } = null!;
    public required string Password { get; set; } = null!;
}
