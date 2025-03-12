using System.ComponentModel.DataAnnotations;

namespace BloggingPlatformApi.Models;

public class RegisterModel
{
    public required string Username { get; set; } = null!;
    [MinLength(6)]
    public required string Password { get; set; } = null!;
    public string Role { get; set; } = "User"; 
}
