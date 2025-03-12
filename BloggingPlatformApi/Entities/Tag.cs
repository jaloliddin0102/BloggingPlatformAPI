namespace BloggingPlatformApi.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public required string Name { get; set; } = string.Empty;

    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
