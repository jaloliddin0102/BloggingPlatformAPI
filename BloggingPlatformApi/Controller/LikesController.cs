using BloggingPlatformApi.Data;
using BloggingPlatformApi.Entities;
using BloggingPlatformApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatformApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LikesController : ControllerBase
{
    private readonly BloggingPlatformContext context;
    private readonly UserManager<IdentityUser> userManager;
    private readonly IHubContext<NotificationHub> hubContext;

    public LikesController(BloggingPlatformContext context, UserManager<IdentityUser> userManager, IHubContext<NotificationHub> hubContext)
    {
        this.context = context;
        this.userManager = userManager;
        this.hubContext = hubContext;
    }

    [HttpPost("{postId}/like")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found.");

        var post = await context.Posts.FindAsync(postId);
        if (post == null)
            return NotFound("Post not found.");

        var existingLike = await context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == user.Id);
        if (existingLike)
            return BadRequest("You have already liked this post.");

        var like = new Like { Id = Guid.NewGuid(), PostId = postId, UserId = user.Id };
        context.Likes.Add(like);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("ReceiveNotification", $"{user.UserName} liked the post: {post.Title}");

        return Ok(new { message = "Liked the post." });
    }

    [HttpDelete("{postId}/like")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]
    public async Task<IActionResult> UnlikePost(Guid postId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found.");

        var like = await context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == user.Id);
        if (like == null)
            return BadRequest("You haven't liked this post.");

        context.Likes.Remove(like);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
