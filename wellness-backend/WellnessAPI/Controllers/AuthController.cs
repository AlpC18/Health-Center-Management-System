using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly EmailService _emailService;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> um,
        TokenService ts,
        ApplicationDbContext db,
        EmailService emailService,
        IConfiguration config,
        IWebHostEnvironment env,
        ILogger<AuthController> logger)
    {
        _userManager = um;
        _tokenService = ts;
        _db = db;
        _emailService = emailService;
        _config = config;
        _env = env;
        _logger = logger;
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
        else if (role == "Therapist")
        {
            var exists = await _db.Terapistet.AnyAsync(t => t.Email == dto.Email);
            if (!exists)
            {
                var terapist = new Terapist
                {
                    Emri = dto.FirstName,
                    Mbiemri = dto.LastName,
                    Email = dto.Email,
                    Specializimi = string.IsNullOrWhiteSpace(dto.Specializimi) ? "General" : dto.Specializimi,
                    Licenca = dto.Licenca,
                    Telefoni = dto.Telefoni,
                    Aktiv = true
                };
                _db.Terapistet.Add(terapist);
                await _db.SaveChangesAsync();
            }
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
    /// <returns>Token-at e autentifikimit (Access dhe Refresh).</returns>
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var genericMessage = "Nese email-i ekziston, do te pranoni nje link per resetimin e fjalekalimit.";
        var email = dto.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return Ok(new { message = genericMessage });

        var activeTokens = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
        foreach (var activeToken in activeTokens)
            activeToken.UsedAt = DateTime.UtcNow;

        var rawToken = PasswordResetToken.CreateRawToken();
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = PasswordResetToken.Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });
        await _db.SaveChangesAsync();

        var frontendBaseUrl = (_config["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var resetLink = $"{frontendBaseUrl}/reset-password/{UrlEncoder.Default.Encode(rawToken)}";
        var body = $@"
            <div style='font-family: Arial, sans-serif; color: #1f2937; line-height: 1.5;'>
                <h2 style='color: #16a34a;'>Resetimi i fjalekalimit</h2>
                <p>Pershendetje {user.FirstName},</p>
                <p>Klikoni butonin me poshte per te vendosur nje fjalekalim te ri. Link-u skadon pas 30 minutash.</p>
                <p>
                    <a href='{resetLink}' style='display: inline-block; padding: 12px 18px; background: #16a34a; color: white; text-decoration: none; border-radius: 8px; font-weight: 700;'>
                        Reset Password
                    </a>
                </p>
                <p>Nese butoni nuk hapet, kopjoni kete link:</p>
                <p style='word-break: break-all; color: #2563eb;'>{resetLink}</p>
                <p>Nese nuk e kerkuat kete ndryshim, mund ta injoroni kete email.</p>
            </div>";

        try
        {
            await _emailService.SendEmailAsync(user.Email!, "Reset Password - Wellness House", body);
        }
        catch (Exception ex)
        {
            // Keep the response generic so attackers cannot discover registered emails.
            if (_env.IsDevelopment())
                _logger.LogWarning(ex, "Password reset email failed. Development reset link: {ResetLink}", resetLink);
        }

        return Ok(new { message = genericMessage });
    }

    [HttpGet("reset-password/{token}/validate")]
    public async Task<IActionResult> ValidateResetToken(string token)
    {
        var tokenHash = PasswordResetToken.Hash(token);
        var exists = await _db.PasswordResetTokens
            .AnyAsync(t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);

        return exists
            ? Ok(new { valid = true })
            : BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new { message = "Fjalekalimet nuk perputhen." });
        if (dto.NewPassword.Length < 8 || !dto.NewPassword.Any(char.IsDigit))
            return BadRequest(new { message = "Fjalekalimi duhet te kete te pakten 8 karaktere dhe nje numer." });

        var tokenHash = PasswordResetToken.Hash(dto.Token);
        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);

        if (resetToken is null)
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });

        var passwordErrors = new List<IdentityError>();
        foreach (var validator in _userManager.PasswordValidators)
        {
            var validation = await validator.ValidateAsync(_userManager, resetToken.User, dto.NewPassword);
            if (!validation.Succeeded)
                passwordErrors.AddRange(validation.Errors);
        }
        if (passwordErrors.Count > 0)
            return BadRequest(new { errors = passwordErrors.Select(e => e.Description) });

        resetToken.User.PasswordHash = _userManager.PasswordHasher.HashPassword(resetToken.User, dto.NewPassword);
        await _userManager.UpdateSecurityStampAsync(resetToken.User);
        var updateResult = await _userManager.UpdateAsync(resetToken.User);
        if (!updateResult.Succeeded)
            return BadRequest(new { errors = updateResult.Errors.Select(e => e.Description) });

        resetToken.UsedAt = DateTime.UtcNow;
        await _db.PasswordResetTokens
            .Where(t => t.UserId == resetToken.UserId && t.UsedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.UsedAt, DateTime.UtcNow));
        await _tokenService.RevokeAllTokensAsync(resetToken.UserId);

        return Ok(new { message = "Fjalekalimi u ndryshua me sukses. Mund te kycesh tani." });
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
