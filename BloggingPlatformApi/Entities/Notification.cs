using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BloggingPlatformApi.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;
    public required IdentityUser User { get; set; }

    [MaxLength(500)]
    public required string Message { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
