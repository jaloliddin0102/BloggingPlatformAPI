using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BloggingPlatformApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BloggingPlatformApi.Services;

public class AuthService
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly IConfiguration configuration;

    public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.configuration = configuration;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterModel model)
    {
        var user = new IdentityUser { UserName = model.Username };
        var result = await userManager.CreateAsync(user, model.Password!);

        if (result.Succeeded)
        {
            string role = model.Role?.ToLower() ?? "user";

            if (await roleManager.RoleExistsAsync(role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                await userManager.AddToRoleAsync(user, "user");
            }
        }

        return result;
    }

    public async Task<string?> LoginAsync(LoginModel model)
    {
        var user = await userManager.FindByNameAsync(model.Username);
        if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
        {
            return null;
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = GenerateJwtToken(authClaims);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private JwtSecurityToken GenerateJwtToken(IEnumerable<Claim> claims)
    {
        var secretKey = configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new ArgumentNullException(nameof(secretKey), "JWT SecretKey null yoki bo‘sh!");
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var expirationMinutes = configuration.GetValue<int>("Jwt:ExpirationMinutes", 180); 

        return new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
    }
}
