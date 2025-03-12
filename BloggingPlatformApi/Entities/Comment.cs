using Microsoft.AspNetCore.Identity;

namespace BloggingPlatformApi.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public required string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid PostId { get; set; } 
    public  Post? Post { get; set; }

    public string UserId { get; set; } = null!;
    public  IdentityUser? User { get; set; }
}
