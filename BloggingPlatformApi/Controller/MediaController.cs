using BloggingPlatformApi.Data;
using BloggingPlatformApi.Entities;
using BloggingPlatformApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggingPlatformApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaController : ControllerBase
{
    private readonly BloggingPlatformContext context;
    private readonly IWebHostEnvironment environment;

    public MediaController(BloggingPlatformContext context, IWebHostEnvironment environment)
    {
        this.context = context;
        this.environment = environment;
    }

    [HttpPost("{postId}/media")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorPolicy")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadMedia(Guid postId, IFormFile file, [FromForm] string fileType)
    {
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
            return NotFound(new { message = "Post not found" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid or missing user ID" });

        var isAdmin = User.IsInRole("admin");
        if (post.UserId.ToString() != userId.ToString() && !isAdmin)
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is empty" });

        if (file.Length > 10 * 1024 * 1024) 
            return BadRequest(new { message = "File size exceeds 10MB limit" });

        var allowedFileTypes = new[] { "image", "video" };
        if (!allowedFileTypes.Contains(fileType.ToLower()))
            return BadRequest(new { message = "Invalid file type. Allowed types: image, video" });

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var media = new MediaAttachment
        {
            Id = Guid.NewGuid(),
            FileUrl = $"/uploads/{uniqueFileName}",
            FileType = Enum.TryParse<FileType>(fileType, true, out var parsedFileType) ? parsedFileType : FileType.Image,
            PostId = postId
        };

        context.MediaAttachments.Add(media);
        await context.SaveChangesAsync();

        return Created($"/uploads/{uniqueFileName}", new
        {
            message = "File uploaded successfully",
            url = $"/uploads/{uniqueFileName}"
        });
    }
}
