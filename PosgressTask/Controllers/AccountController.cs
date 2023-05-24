using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PosgressTask.Context;

namespace PosgressTask.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    
    private string GenerateToken(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Name ,user.Username)
        };
        var signInKey = System.Text.Encoding.UTF32.GetBytes("qwertyuiopasdfghjklzxcvbnm123456789");
        var security = new JwtSecurityToken(
            issuer: "Identity.Api",
            audience: "Products",
            expires: DateTime.Now.AddHours(10),
            claims: claims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(signInKey),
                SecurityAlgorithms.HmacSha256)
        );
        
        var token= new JwtSecurityTokenHandler().WriteToken(security);
        return token;
    }
    
    [HttpPost("register")]
    public async Task<Guid> Register(string username, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            throw new Exception($"User exists with username: {username}");
        }

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Username = username,
            Password = password,
            Roles = new List<Role>() { Role.Admin }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user.Id;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            throw new Exception("Username or password is incorrect");
        }

        return Ok(new
        {
            Token = GenerateToken(user)
        });
    }
    [Authorize]
    [HttpGet]
    public IActionResult Profile()
    {
        return Ok(User.FindFirstValue(ClaimTypes.Name));
    }
}