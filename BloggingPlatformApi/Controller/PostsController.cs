using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BloggingPlatformApi.Data;
using BloggingPlatformApi.Models;
using BloggingPlatformApi.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using BloggingPlatformApi.Hubs;
namespace BloggingPlatformApi.Controllers;

[Route("api/posts")]
[ApiController]
public class PostsController(
        BloggingPlatformContext context,
        UserManager<IdentityUser> userManager,
        IHubContext<NotificationHub> hubContext) : ControllerBase
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorPolicy")]
    public async Task<IActionResult> CreatePost([FromBody] PostRequestModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var post = new Post
        {
            Title = model.Title,
            Content = model.Content,
            Status = Enum.Parse<PostStatus>(model.Status),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            CategoryId = model.CategoryId,
            PostTags = [.. model.TagIds.Select(tagId => new PostTag { TagId = tagId })]
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();
        await hubContext.Clients.All.SendAsync("ReceiveNotification", $"{user.UserName} added a new post: {post.Title}");
        return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
    }

    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorPolicy")]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        var user = await userManager.GetUserAsync(User);
        var isAdmin = await userManager.IsInRoleAsync(user!, "Admin");

        var post = await context.Posts
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        if (!isAdmin && post.UserId != user!.Id) return Forbid();

        return Ok(post);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorPolicy")]
    public async Task<IActionResult> GetPosts([FromQuery] string? status, [FromQuery] Guid? categoryId, [FromQuery] Guid? tagId, [FromQuery] string? search)
    {
        var user = await userManager.GetUserAsync(User);
        var isAdmin = await userManager.IsInRoleAsync(user!, "Admin");

        var query = context.Posts
            .Include(p => p.PostTags)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(p => p.UserId == user!.Id);
        }

        if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == Enum.Parse<PostStatus>(status));
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (tagId.HasValue) query = query.Where(p => p.PostTags.Any(pt => pt.TagId == tagId));
        if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));

        var posts = await query.ToListAsync();
        return Ok(posts);
    }

    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorPolicy")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] PostRequestModel model)
    {
        var post = await context.Posts
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        var user = await userManager.GetUserAsync(User);
        var isAdmin = await userManager.IsInRoleAsync(user!, "Admin");

        if (post.UserId != user!.Id && !isAdmin) return Forbid();

        post.Title = model.Title;
        post.Content = model.Content;
        post.Status = Enum.Parse<PostStatus>(model.Status);
        post.PublishedAt = model.PublishedAt;
        post.CategoryId = model.CategoryId;

        context.PostTags.RemoveRange(post.PostTags);
        post.PostTags = [.. model.TagIds.Select(tagId => new PostTag { PostId = id, TagId = tagId })];

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorPolicy")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var post = await context.Posts
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        var user = await userManager.GetUserAsync(User);
        var isAdmin = await userManager.IsInRoleAsync(user!, "Admin");

        if (post.UserId != user!.Id && !isAdmin) return Forbid();

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
