namespace BloggingPlatformApi.Models;

public class PostRequestModel
{
    public required string Title { get; set; } = null!;
    public required string Content { get; set; } = null!;
    public required string Status { get; set; } = "Draft"; 
    public Guid CategoryId { get; set; }
    public List<Guid> TagIds { get; set; } = new();
    public DateTime? PublishedAt { get; set; }
}
