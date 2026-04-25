using System.Security.Cryptography;

namespace WellnessAPI.Models.Identity;

public class PasswordResetToken
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public bool IsActive => UsedAt is null && ExpiresAt > DateTime.UtcNow;

    public static string CreateRawToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
