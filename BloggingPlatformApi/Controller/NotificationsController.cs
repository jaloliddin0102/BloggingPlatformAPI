using BloggingPlatformApi.Data;
using BloggingPlatformApi.Entities; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatformApi.Controller;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly BloggingPlatformContext context;
    private readonly UserManager<IdentityUser> userManager;

    public NotificationsController(BloggingPlatformContext context, UserManager<IdentityUser> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]
    public async Task<IActionResult> GetNotifications()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) 
            return Unauthorized("User not found.");

        var notifications = await context.Notifications
            .Where(n => n.UserId == user.Id) 
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorModeratorUserPolicy")]
    public async Task<IActionResult> MarkNotificationAsRead(Guid id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) 
            return Unauthorized("User not found.");

        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

        if (notification == null) 
            return NotFound("Notification not found or does not belong to you.");

        notification.IsRead = true;
        context.Notifications.Update(notification);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}
