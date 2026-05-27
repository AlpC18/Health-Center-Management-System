using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Identity;

namespace WellnessAPI.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(IConfiguration config, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _config = config;
        _db = db;
        _userManager = userManager;
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var configuredExpiryMinutes)
            ? configuredExpiryMinutes
            : 15;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(RefreshToken StoredToken, string RawToken)> GenerateRefreshTokenAsync(ApplicationUser user, string? ip)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashedToken = HashRefreshToken(rawToken);

        var token = new RefreshToken
        {
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ip,
            UserId = user.Id
        };

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return (token, rawToken);
    }

    public async Task<(string AccessToken, RefreshToken NewStoredRefreshToken, string NewRawRefreshToken)> RotateRefreshTokenAsync(string oldRawToken, string? ip)
    {
        var oldHash = HashRefreshToken(oldRawToken);
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == oldHash)
            ?? throw new UnauthorizedAccessException("Token i pavlefshem.");

        if (!stored.IsActive)
            throw new UnauthorizedAccessException("Token ka skaduar ose eshte revokuar.");

        stored.RevokedAt = DateTime.UtcNow;
        var (newStoredRefresh, newRawRefresh) = await GenerateRefreshTokenAsync(stored.User, ip);
        var newAccess = await GenerateAccessTokenAsync(stored.User);
        await _db.SaveChangesAsync();

        return (newAccess, newStoredRefresh, newRawRefresh);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string rawToken, string userId)
    {
        var tokenHash = HashRefreshToken(rawToken);
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == tokenHash && r.UserId == userId);

        if (token is null || !token.IsActive)
        {
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task RevokeAllTokensAsync(string userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public AuthResponseDto BuildAuthResponse(ApplicationUser user, string access, string rawRefreshToken, DateTime refreshExpiresAt, string role) =>
        new(access, rawRefreshToken, refreshExpiresAt,
            new UserInfoDto(user.Id, user.Email ?? "", user.FirstName, user.LastName, role, user.PhoneNumber, user.Adresa, user.TwoFactorEnabled));

    private string HashRefreshToken(string rawToken)
    {
        var secret = _config["Jwt:Key"]!;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash);
    }
}
