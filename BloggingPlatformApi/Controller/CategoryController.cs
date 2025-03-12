using BloggingPlatformApi.Data;
using BloggingPlatformApi.Entities;
using BloggingPlatformApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatformApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminAuthorPolicy")]
public class CategoriesController(BloggingPlatformContext context, UserManager<IdentityUser> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var category = new Category
        {
            Name = model.Name
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        bool isAdmin = User.IsInRole("Admin");

        var categoryQuery = context.Categories
            .Include(c => c.Posts)
            .AsQueryable();

        if (!isAdmin)
        {
            categoryQuery = categoryQuery
                .Where(c => c.Posts.Any(p => p.UserId == user.Id));
        }

        var category = await categoryQuery.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return NotFound();
        return Ok(category);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        bool isAdmin = User.IsInRole("Admin");

        var categoriesQuery = context.Categories
            .Include(c => c.Posts)
            .AsQueryable();

        if (!isAdmin)
        {
            categoriesQuery = categoriesQuery
                .Where(c => c.Posts.Any(p => p.UserId == user.Id));
        }

        var categories = await categoriesQuery.ToListAsync();
        return Ok(categories);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        bool isAdmin = User.IsInRole("Admin");

        var categoryQuery = context.Categories
            .Include(c => c.Posts)
            .AsQueryable();

        if (!isAdmin)
        {
            categoryQuery = categoryQuery
                .Where(c => c.Posts.Any(p => p.UserId == user.Id));
        }

        var category = await categoryQuery.FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        return NoContent();
    }

}