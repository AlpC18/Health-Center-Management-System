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
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> um,
        TokenService ts,
        ApplicationDbContext db,
        IWebHostEnvironment env,
        EmailService emailService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = um;
        _tokenService = ts;
        _db = db;
        _env = env;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Confirms a user's email from the link sent at registration.</summary>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return BadRequest(new { message = "Perdoruesi nuk u gjet." });
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? Ok(new { message = "Email-i u konfirmua me sukses." })
            : BadRequest(new { message = "Konfirmimi deshtoi ose linku ka skaduar." });
    }

    // Best-effort: send an email-confirmation link. Never blocks registration
    // (SMTP may be unconfigured in dev), so failures are logged, not thrown.
    private async Task SendEmailConfirmationAsync(ApplicationUser user)
    {
        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = $"http://localhost:5077/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendEmailAsync(user.Email!,
                "Konfirmo email-in - Wellness House",
                $"Pershendetje {user.FirstName},\n\nKonfirmo email-in duke klikuar: {link}\n\nWellness House");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email i konfirmimit deshtoi per {Email}", user.Email);
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return BadRequest(new { success = false, message = "EXISTING_ACCOUNT", text = "Kjo llogari ekziston. Ju lutem hyni ne sistem." });

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

        await SendEmailConfirmationAsync(user);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var access = await _tokenService.GenerateAccessTokenAsync(user);
        var refresh = await _tokenService.GenerateRefreshTokenAsync(user, ip);

        WriteRefreshCookie(refresh.RawToken, refresh.StoredToken.ExpiresAt);

        var authResponse = _tokenService.BuildAuthResponse(
            user,
            access,
            refresh.RawToken,
            refresh.StoredToken.ExpiresAt,
            role);
        return Ok(CompatAuthResponse(authResponse));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized(new { message = "Email ose fjalekalim i gabuar." });

        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized(new { message = "Llogaria eshte bllokuar perkohesisht (shume perpjekje). Provoni me vone." });

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            await _userManager.AccessFailedAsync(user);   // count toward lockout
            return Unauthorized(new { message = "Email ose fjalekalim i gabuar." });
        }
        await _userManager.ResetAccessFailedCountAsync(user);   // success -> reset counter

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Klient";

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var access = await _tokenService.GenerateAccessTokenAsync(user);
        var refresh = await _tokenService.GenerateRefreshTokenAsync(user, ip);

        WriteRefreshCookie(refresh.RawToken, refresh.StoredToken.ExpiresAt);

        var authResponse = _tokenService.BuildAuthResponse(
            user,
            access,
            refresh.RawToken,
            refresh.StoredToken.ExpiresAt,
            role);
        return Ok(CompatAuthResponse(authResponse));
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

            var authResponse = _tokenService.BuildAuthResponse(
                user,
                rotation.AccessToken,
                rotation.NewRawRefreshToken,
                rotation.NewStoredRefreshToken.ExpiresAt,
                role);
            return Ok(CompatAuthResponse(authResponse));
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
        return Ok(new { success = true, message = "Ckycja u krye." });
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
        return Ok(new { success = true, message = "Fjalekalimi u ndryshua." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Klient";
        var userInfo = new UserInfoDto(user.Id, user.Email ?? "", user.FirstName, user.LastName, role, user.PhoneNumber, user.Adresa);
        return Ok(CompatUserResponse(userInfo));
    }


    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        if (user is null) return Unauthorized();

        user.PhoneNumber = dto.Telefoni;
        user.Adresa = dto.Adresa;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Klient";
        var access = await _tokenService.GenerateAccessTokenAsync(user);
        var refresh = await _tokenService.GenerateRefreshTokenAsync(user, HttpContext.Connection.RemoteIpAddress?.ToString());
        WriteRefreshCookie(refresh.RawToken, refresh.StoredToken.ExpiresAt);

        var authResponse = _tokenService.BuildAuthResponse(user, access, refresh.RawToken, refresh.StoredToken.ExpiresAt, role);
        return Ok(CompatAuthResponse(authResponse));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Email eshte i detyrueshem." });

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        // Always return the same response to avoid account enumeration.
        if (user is null)
            return Ok(new { message = "Nese email-i ekziston, link-u i resetimit u dergua." });

        var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var combinedToken = $"{user.Id}:{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rawToken))}";
        var encodedToken = Uri.EscapeDataString(combinedToken);
        var frontendBaseUrl = (_configuration["App:FrontendBaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var resetUrl = $"{frontendBaseUrl}/reset-password/{encodedToken}";

        var subject = "Resetimi i fjalekalimit";
        var body = $@"
            <p>Pershendetje {user.FirstName ?? user.Email},</p>
            <p>Keni kerkuar resetim te fjalekalimit. Klikoni linkun me poshte:</p>
            <p><a href='{resetUrl}'>Reset Password</a></p>
            <p>Link-u skadon pas nje periudhe te shkurter sigurie.</p>
            <p>Nese nuk e keni kerkuar ju, injoroni kete email.</p>";

        try
        {
            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }
        catch
        {
            // Do not fail the endpoint because of SMTP/config issues.
        }

        return Ok(new { message = "Nese email-i ekziston, link-u i resetimit u dergua." });
    }

    [HttpGet("reset-password/{token}/validate")]
    public async Task<IActionResult> ValidateResetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Token mungon." });

        var decodedToken = Uri.UnescapeDataString(token);
        var split = decodedToken.Split(':', 2);
        if (split.Length != 2)
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });

        string userId;
        string rawToken;
        try
        {
            userId = split[0];
            rawToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(split[1]));
        }
        catch
        {
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });

        var verificationResult = await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            "ResetPassword",
            rawToken);

        if (!verificationResult)
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });

        return Ok(new { message = "Token valid." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(new { message = "Token mungon." });

        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new { message = "Fjalekalimet nuk perputhen." });

        var decodedToken = Uri.UnescapeDataString(dto.Token);
        var split = decodedToken.Split(':', 2);
        if (split.Length != 2)
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });

        string userId;
        string rawToken;
        try
        {
            userId = split[0];
            rawToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(split[1]));
        }
        catch
        {
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest(new { message = "Link-u per resetim eshte i pavlefshem ose ka skaduar." });

        var resetResult = await _userManager.ResetPasswordAsync(user, rawToken, dto.NewPassword);
        if (!resetResult.Succeeded)
            return BadRequest(new { errors = resetResult.Errors.Select(e => e.Description) });

        await _tokenService.RevokeAllTokensAsync(user.Id);
        DeleteRefreshCookie();
        return Ok(new { message = "Fjalekalimi u resetua me sukses." });
    }
    private string? ResolveRefreshToken(RefreshTokenRequestDto? dto)
    {
        if (!string.IsNullOrWhiteSpace(dto?.RefreshToken))
            return dto.RefreshToken;

        if (Request.Cookies.TryGetValue(RefreshCookieName, out var cookieToken))
            return cookieToken;

        return null;
    }

    private static object CompatAuthResponse(AuthResponseDto response) => new
    {
        success = true,
        accessToken = response.AccessToken,
        refreshToken = response.RefreshToken,
        expiresAt = response.ExpiresAt,
        user = response.User,
        data = new Dictionary<string, object?>
        {
            ["AccessToken"] = response.AccessToken,
            ["RefreshToken"] = response.RefreshToken,
            ["ExpiresAt"] = response.ExpiresAt,
            ["User"] = CompatUserData(response.User)
        }
    };

    private static object CompatUserResponse(UserInfoDto user) => new
    {
        success = true,
        user,
        data = CompatUserData(user)
    };

    private static Dictionary<string, object?> CompatUserData(UserInfoDto user) => new()
    {
        ["Id"] = user.Id,
        ["Email"] = user.Email,
        ["FirstName"] = user.FirstName,
        ["LastName"] = user.LastName,
        ["Role"] = user.Role,
        ["Telefoni"] = user.Telefoni,
        ["Adresa"] = user.Adresa
    };

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
