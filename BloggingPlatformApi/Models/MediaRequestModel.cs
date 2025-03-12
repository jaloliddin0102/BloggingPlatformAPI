namespace BloggingPlatformApi.Models;

public class MediaRequestModel
{
    public required IFormFile File { get; set; }
    public string FileType { get; set; } = "image";
}
