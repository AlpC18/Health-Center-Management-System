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

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "wellness_rt";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AuthController(
        UserManager<ApplicationUser> um,
        TokenService ts,
        ApplicationDbContext db,
        IWebHostEnvironment env)
    {
        _userManager = um;
        _tokenService = ts;
        _db = db;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Conflict(new { message = "EXISTING_ACCOUNT", text = "Kjo llogari ekziston. Ju lutem hyni ne sistem." });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
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

        WriteRefreshCookie(refresh.RawToken, refresh.StoredToken.ExpiresAt);

        return Ok(_tokenService.BuildAuthResponse(
            user,
            access,
            refresh.RawToken,
            refresh.StoredToken.ExpiresAt,
            role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Email ose fjalekalim i gabuar." });

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Klient";

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var access = await _tokenService.GenerateAccessTokenAsync(user);
        var refresh = await _tokenService.GenerateRefreshTokenAsync(user, ip);

        WriteRefreshCookie(refresh.RawToken, refresh.StoredToken.ExpiresAt);

        return Ok(_tokenService.BuildAuthResponse(
            user,
            access,
            refresh.RawToken,
            refresh.StoredToken.ExpiresAt,
            role));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto? dto)
    {
        var rawRefreshToken = ResolveRefreshToken(dto);
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            return Unauthorized(new { message = "Refresh token mungon." });

        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var rotation = await _tokenService.RotateRefreshTokenAsync(rawRefreshToken, ip);

            var user = await _userManager.FindByIdAsync(rotation.NewStoredRefreshToken.UserId);
            if (user is null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Klient";

            WriteRefreshCookie(rotation.NewRawRefreshToken, rotation.NewStoredRefreshToken.ExpiresAt);

            return Ok(_tokenService.BuildAuthResponse(
                user,
                rotation.AccessToken,
                rotation.NewRawRefreshToken,
                rotation.NewStoredRefreshToken.ExpiresAt,
                role));
        }
        catch (UnauthorizedAccessException ex)
        {
            DeleteRefreshCookie();
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var rawRefreshToken = ResolveRefreshToken(dto);
        if (!string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            await _tokenService.RevokeRefreshTokenAsync(rawRefreshToken, userId);
        }

        DeleteRefreshCookie();
        return Ok(new { message = "Ckycja u krye." });
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        if (user is null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _tokenService.RevokeAllTokensAsync(user.Id);
        DeleteRefreshCookie();
        return Ok(new { message = "Fjalekalimi u ndryshua." });
    }

    private string? ResolveRefreshToken(RefreshTokenRequestDto? dto)
    {
        if (!string.IsNullOrWhiteSpace(dto?.RefreshToken))
            return dto.RefreshToken;

        if (Request.Cookies.TryGetValue(RefreshCookieName, out var cookieToken))
            return cookieToken;

        return null;
    }

    private void WriteRefreshCookie(string refreshToken, DateTime expiresAt)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            IsEssential = true,
            Path = "/"
        });
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/" });
    }
}
