using Microsoft.AspNetCore.Identity;
namespace WellnessAPI.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? KlientId { get; set; }
    // Links the Identity user to a Terapist domain row when the user is in role "Therapist".
    public string? TerapistId { get; set; }
    public string? Adresa { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
