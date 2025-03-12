using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BloggingPlatformApi.Entities;

public class Post
{
    public Guid Id { get; set; }

    [MaxLength(255)]
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required PostStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    public string UserId { get; set; } = null!;
    public IdentityUser? User { get; set; }

    public Guid CategoryId { get; set; } 
    public Category? Category { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<MediaAttachment> MediaAttachments { get; set; } = new List<MediaAttachment>();
}

public enum PostStatus
{
    Draft,
    Published
}
