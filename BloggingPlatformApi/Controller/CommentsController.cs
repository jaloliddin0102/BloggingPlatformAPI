using BloggingPlatformApi.Data;
using BloggingPlatformApi.Entities;
using BloggingPlatformApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BloggingPlatformApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly BloggingPlatformContext _context;

    public CommentsController(BloggingPlatformContext context)
    {
        _context = context;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]    
    [HttpGet("post/{postId}")]
    public async Task<IActionResult> GetCommentsByPost(Guid postId)
    {
        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return Ok(comments);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComment(Guid id)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null)
            return NotFound();
        return Ok(comment);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]    
    [HttpPost("post/{postId}")]
    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CommentRequestModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("Token is missing user ID.");

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid user ID format.");

        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
            return NotFound("Post not found.");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = model.Content,
            CreatedAt = DateTime.UtcNow,
            PostId = postId,
            UserId = userId.ToString()
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] CommentRequestModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return NotFound();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("Token is missing user ID.");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid user ID format.");

        var role = User.FindFirstValue(ClaimTypes.Role);
        bool isAdminOrModerator = role == "Admin" || role == "Moderator";
        if (!isAdminOrModerator && comment.UserId.ToString() != userId.ToString())
            return Forbid("You cannot edit someone else's comment.");

        comment.Content = model.Content;
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return NotFound();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("Token is missing user ID.");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid user ID format.");

        var role = User.FindFirstValue(ClaimTypes.Role);
        bool isAdminOrModerator = role == "Admin" || role == "Moderator";
        if (!isAdminOrModerator && comment.UserId.ToString() != userId.ToString())
            return Forbid("You cannot delete someone else's comment.");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
