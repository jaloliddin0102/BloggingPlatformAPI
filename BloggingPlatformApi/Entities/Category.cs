namespace BloggingPlatformApi.Entities;
public class Category
{
    public Guid Id { get; set; }
    public required string Name { get; set; } = null!;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
