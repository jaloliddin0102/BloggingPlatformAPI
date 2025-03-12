using Microsoft.AspNetCore.Identity;

namespace BloggingPlatformApi.Entities;

public class Like
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post? Post { get; set; }

    public string UserId { get; set; } = null!;
    public IdentityUser? User { get; set; }
}
