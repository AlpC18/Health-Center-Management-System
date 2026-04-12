using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    public TokenService(IConfiguration config, ApplicationDbContext db)
    {
        _config = config;
        _db = db;
    }

    public string GenerateAccessToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string? ip)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(tokenBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip,
            UserId = user.Id
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<(string AccessToken, RefreshToken NewRefreshToken)?> RotateRefreshTokenAsync(string oldToken, string? ip)
    {
        var existing = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == oldToken);

        if (existing == null || !existing.IsActive) return null;

        existing.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var newRefreshToken = await GenerateRefreshTokenAsync(existing.User, ip);
        var accessToken = GenerateAccessToken(existing.User);
        return (accessToken, newRefreshToken);
    }

    public async Task RevokeAllTokensAsync(string userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public AuthResponseDto BuildAuthResponse(ApplicationUser user, string accessToken, RefreshToken refreshToken)
    {
        return new AuthResponseDto(
            accessToken,
            refreshToken.Token,
            new UserInfoDto(user.Id, user.Email!, user.FirstName, user.LastName)
        );
    }
}
