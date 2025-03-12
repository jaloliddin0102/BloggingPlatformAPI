using System.ComponentModel.DataAnnotations;

namespace BloggingPlatformApi.Entities;

public class MediaAttachment
{
    public Guid Id { get; set; }

    [MaxLength(500)]
    public required string FileUrl { get; set; }
    public required FileType FileType { get; set; }

    public Guid PostId { get; set; } 
    public  Post? Post { get; set; }

  
}

public enum FileType
{
    Image,
    Video,
    Audio,
    Document
}
