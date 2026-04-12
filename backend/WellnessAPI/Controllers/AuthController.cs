using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Models.Identity;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

/// <summary>
/// Kontrolluesi për autentifikimin e përdoruesve (Register, Login, Refresh, Password).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly ApplicationDbContext _db;

    public AuthController(UserManager<ApplicationUser> um, TokenService ts, ApplicationDbContext db)
    {
        _userManager = um;
        _tokenService = ts;
        _db = db;
    }

    /// <summary>
    /// Regjistron një përdorues të ri në sistem.
    /// </summary>
    /// <param name="dto">Të dhënat për regjistrim.</param>
    /// <returns>Token-at e autentifikimit dhe të dhënat e përdoruesit.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Conflict(new { message = "EXISTING_ACCOUNT", text = "Kjo llogari ekziston. Ju lutem hyni në sistem." });

        var user = new ApplicationUser
        {
            UserName = dto.Email, Email = dto.Email,
            FirstName = dto.FirstName, LastName = dto.LastName
        };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        var role = dto.Role == "Admin" || dto.Role == "Therapist" ? dto.Role : "Klient";
        await _userManager.AddToRoleAsync(user, role);

        if (role == "Klient")
        {
            var klient = new Klient
            {
                Emri = dto.FirstName,
                Mbiemri = dto.LastName,
                Email = dto.Email,
                DataRegjistrimit = DateTime.UtcNow
            };
            _db.Klientet.Add(klient);
            await _db.SaveChangesAsync();
            user.KlientId = klient.KlientId.ToString();
            await _userManager.UpdateAsync(user);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var access = await _tokenService.GenerateAccessTokenAsync(user);
        var refresh = await _tokenService.GenerateRefreshTokenAsync(user, ip);
        return Ok(_tokenService.BuildAuthResponse(user, access, refresh, role));
    }

    /// <summary>
    /// Kyç një përdorues ekzistues dhe gjeneron token-at JWT.
    /// </summary>
    /// <param name="dto">Kredencialet (Email dhe Password).</param>
    /// <returns>Token-at e autentifikimit (Access & Refresh).</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Email ose fjalëkalim i gabuar." });

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Klient";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var access = await _tokenService.GenerateAccessTokenAsync(user);
        var refresh = await _tokenService.GenerateRefreshTokenAsync(user, ip);
        return Ok(_tokenService.BuildAuthResponse(user, access, refresh, role));
    }

    /// <summary>
    /// Rifreskon Access Token duke përdorur një Refresh Token valid.
    /// </summary>
    /// <param name="dto">Refresh token aktual.</param>
    /// <returns>Një Access Token i ri dhe Refresh Token i rrotulluar.</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(
        [FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (access, refresh) =
                await _tokenService.RotateRefreshTokenAsync(dto.RefreshToken, ip);
            var user = await _userManager.FindByIdAsync(refresh.UserId);
            if (user is null) return Unauthorized();
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Klient";
            return Ok(_tokenService.BuildAuthResponse(user, access, refresh, role));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<Data.ApplicationDbContext>();
        var token = db.RefreshTokens
            .FirstOrDefault(r => r.Token == dto.RefreshToken);
        if (token?.IsActive == true)
        {
            token.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        return Ok(new { message = "Çkyçja u krye." });
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        if (user is null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(
            user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _tokenService.RevokeAllTokensAsync(user.Id);
        return Ok(new { message = "Fjalëkalimi u ndryshua." });
    }
}
