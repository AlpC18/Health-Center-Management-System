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
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
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
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(
        ApplicationUser user, string? ip)
    {
        var token = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ip,
            UserId = user.Id
        };
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return token;
    }

    public async Task<(string AccessToken, RefreshToken NewRefreshToken)>
        RotateRefreshTokenAsync(string oldToken, string? ip)
    {
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == oldToken)
            ?? throw new UnauthorizedAccessException("Token i pavlefshëm.");

        if (!stored.IsActive)
            throw new UnauthorizedAccessException("Token ka skaduar ose është revokuar.");

        stored.RevokedAt = DateTime.UtcNow;
        var newRefresh = await GenerateRefreshTokenAsync(stored.User, ip);
        var newAccess = await GenerateAccessTokenAsync(stored.User);
        await _db.SaveChangesAsync();
        return (newAccess, newRefresh);
    }

    public async Task RevokeAllTokensAsync(string userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();
        foreach (var t in tokens) t.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public AuthResponseDto BuildAuthResponse(
        ApplicationUser user, string access, RefreshToken refresh, string role) =>
        new(access, refresh.Token, refresh.ExpiresAt,
            new UserInfoDto(user.Id, user.Email ?? "", user.FirstName, user.LastName, role));
}
