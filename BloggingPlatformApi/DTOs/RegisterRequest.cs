using System.Text.Json.Serialization;

namespace BloggingPlatformApi.DTOs;
public class RegisterRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public string Role { get; set; } = "User"; 
}

