using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartPortfolio.API.Dtos.Users;
using SmartPortfolio.Application.Users.Commands.LoginUser;
using SmartPortfolio.Application.Users.Commands.RegisterUser;

namespace SmartPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var command = new RegisterUserCommand(dto.Email, dto.Name, dto.Password);
        var userId = await _sender.Send(command);

        return Ok(new { UserId = userId, Message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        var command = new LoginUserCommand(dto.Email, dto.Password);
        var token = await _sender.Send(command);

        return Ok(new { token });
    }
}