using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Identity;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly Dictionary<string, (int Count, DateTime LastAttempt)> _loginAttempts = new();

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ip);
        return Ok(_tokenService.BuildAuthResponse(user, accessToken, refreshToken));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (_loginAttempts.TryGetValue(ip, out var attempt))
        {
            if (attempt.Count >= 5 && DateTime.UtcNow - attempt.LastAttempt < TimeSpan.FromMinutes(15))
                return StatusCode(429, new { message = "Shumë tentativa. Provoni pas 15 minutash." });
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
        {
            _loginAttempts[ip] = _loginAttempts.TryGetValue(ip, out var a) ? (a.Count + 1, DateTime.UtcNow) : (1, DateTime.UtcNow);
            return Unauthorized("Kredencialet jane te gabuara.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            _loginAttempts[ip] = _loginAttempts.TryGetValue(ip, out var a) ? (a.Count + 1, DateTime.UtcNow) : (1, DateTime.UtcNow);
            return Unauthorized("Kredencialet jane te gabuara.");
        }

        _loginAttempts.Remove(ip);
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ip);
        return Ok(_tokenService.BuildAuthResponse(user, accessToken, refreshToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _tokenService.RotateRefreshTokenAsync(dto.RefreshToken, ip);
        if (result == null)
            return Unauthorized("Token i pavlefshëm ose i skaduar.");

        return Ok(new { accessToken = result.Value.AccessToken, refreshToken = result.Value.NewRefreshToken.Token });
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        if (user is null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _tokenService.RevokeAllTokensAsync(user.Id);
        return Ok(new { message = "Fjalëkalimi u ndryshua me sukses." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        if (userId != null)
            await _tokenService.RevokeAllTokensAsync(userId);

        return Ok(new { message = "Dilur me sukses." });
    }
}
