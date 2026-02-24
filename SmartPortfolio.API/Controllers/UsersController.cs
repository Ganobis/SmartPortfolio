using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartPortfolio.API.Dtos.Users;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly SmartPortfolioDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public UsersController(SmartPortfolioDbContext dbContext,  IPasswordHasher<User> passwordHasher, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Email == dto.Email);
        if (userExists)
        {
            return BadRequest("User with this email already exists");
        }

        var dummyUser = new User(dto.Email, "", dto.Name);
        var hashedPassword = _passwordHasher.HashPassword(dummyUser, dto.Password);

        var user = new User(dto.Email, hashedPassword, dto.Name);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok(new { UserId = user.Id, Message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if(user == null)
        {
            return BadRequest("Wrong email or password");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if(result == PasswordVerificationResult.Failed)
        {
            return BadRequest("Wrong email or password");
        }

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
