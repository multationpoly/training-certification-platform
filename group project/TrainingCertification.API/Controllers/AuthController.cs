using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrainingCertification.API.DTOs;
using TrainingCertification.API.Models;
using TrainingCertification.API.Services;

namespace TrainingCertification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwt;
    public AuthController(UserManager<ApplicationUser> userManager, IJwtTokenService jwt) { _userManager = userManager; _jwt = jwt; }

    /// <summary>Authenticates a user and returns a JWT token.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password)) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(await _jwt.CreateTokenAsync(user));
    }

    /// <summary>Registers a public trainee account.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true,
            TraineeNumber = await TraineeNumberGenerator.GenerateUniqueAsync(_userManager)
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });

        await _userManager.AddToRoleAsync(user, Roles.Trainee);
        return CreatedAtAction(nameof(Login), new { email = user.Email }, new { message = "Registration successful.", traineeNumber = user.TraineeNumber });
    }
}
