namespace WellnessAPI.DTOs;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role = "Klient",
    string? Specializimi = null,
    string? Licenca = null,
    string? Telefoni = null,
    bool AcceptedConsent = false,
    string? ConsentVersion = null
);
public record LoginDto(string Email, string Password, string? TwoFactorCode = null);
public record AuthResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserInfoDto User);
public record UserInfoDto(string Id, string Email, string FirstName, string LastName, string Role, string? Telefoni = null, string? Adresa = null, bool TwoFactorEnabled = false);
public record RefreshTokenRequestDto(string RefreshToken);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);
public record UpdateProfileDto(string? Telefoni, string? Adresa);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Token, string NewPassword, string ConfirmPassword);

public record KlientCreateDto(string Emri, string Mbiemri, string Email, string? Telefoni, DateTime? DataLindjes, string? Gjinia, string? KushtetShendetesore, string LoyaltyTier = "Bronze", decimal DiscountPercent = 0);
public record KlientUpdateDto(string Emri, string Mbiemri, string Email, string? Telefoni, DateTime? DataLindjes, string? Gjinia, string? KushtetShendetesore, string LoyaltyTier = "Bronze", decimal DiscountPercent = 0);
public record KlientResponseDto(int KlientId, string Emri, string Mbiemri, string Email, string? Telefoni, DateTime? DataLindjes, string? Gjinia, string? KushtetShendetesore, string? FotoPath, DateTime DataRegjistrimit, string LoyaltyTier = "Bronze", decimal DiscountPercent = 0);

public record SherbimCreateDto(string EmriSherbimit, string? Kategoria, string? Pershkrimi, int KohezgjatjaMin, decimal Cmimi, bool Aktiv = true);
public record SherbimUpdateDto(string EmriSherbimit, string? Kategoria, string? Pershkrimi, int KohezgjatjaMin, decimal Cmimi, bool Aktiv);
public record SherbimResponseDto(int SherbimId, string EmriSherbimit, string? Kategoria, string? Pershkrimi, int KohezgjatjaMin, decimal Cmimi, bool Aktiv);

public record TerapistCreateDto(string Emri, string Mbiemri, string? Specializimi, string? Licenca, string Email, string? Telefoni, bool Aktiv = true, string? UserId = null, int? LokacioniId = null);
public record TerapistUpdateDto(string Emri, string Mbiemri, string? Specializimi, string? Licenca, string Email, string? Telefoni, bool Aktiv, string? UserId = null, int? LokacioniId = null);
public record TerapistResponseDto(int TerapistId, string Emri, string Mbiemri, string? Specializimi, string? Licenca, string Email, string? Telefoni, bool Aktiv, string? UserId = null, int? LokacioniId = null, string? LokacioniEmri = null);

public record TerminCreateDto(int KlientId, int SherbimId, int TerapistId, DateTime DataTerminit, TimeSpan OraFillimit, TimeSpan OraMbarimit, string? Statusi, string? Shenimet, int? LokacioniId = null);
public record TerminUpdateDto(int KlientId, int SherbimId, int TerapistId, DateTime DataTerminit, TimeSpan OraFillimit, TimeSpan OraMbarimit, string Statusi, string? Shenimet, int? LokacioniId = null);
public record TerminResponseDto(int TerminId, int KlientId, string KlientEmri, int SherbimId, string SherbimEmri, int TerapistId, string TerapistEmri, DateTime DataTerminit, TimeSpan OraFillimit, TimeSpan OraMbarimit, string Statusi, string? Shenimet, int? LokacioniId = null, string? LokacioniEmri = null, DateTime? ProposedStart = null, DateTime? ProposedEnd = null, string? RescheduleNote = null);
public record PortalTerminCreateDto(int SherbimId, int TerapistId, DateTime DataTerminit, TimeSpan OraFillimit, TimeSpan OraMbarimit, string? Statusi, string? Shenimet, int? LokacioniId = null);

public record PaketaCreateDto(string EmriPaketes, string? Pershkrimi, string? SherbimiPerfshire, decimal Cmimi, int KohezgjatjaMuaj, bool Aktive = true);
public record PaketaUpdateDto(string EmriPaketes, string? Pershkrimi, string? SherbimiPerfshire, decimal Cmimi, int KohezgjatjaMuaj, bool Aktive);
public record PaketaResponseDto(int PaketId, string EmriPaketes, string? Pershkrimi, string? SherbimiPerfshire, decimal Cmimi, int KohezgjatjaMuaj, bool Aktive);

public record AnetaresimCreateDto(int KlientId, int PaketId, DateTime DataFillimit, DateTime DataMbarimit, string Statusi, decimal CmimiPaguar);
public record AnetaresimUpdateDto(int KlientId, int PaketId, DateTime DataFillimit, DateTime DataMbarimit, string Statusi, decimal CmimiPaguar);
public record AnetaresimResponseDto(int AnetaresimId, int KlientId, string KlientEmri, int PaketId, string PaketaEmri, DateTime DataFillimit, DateTime DataMbarimit, string Statusi, decimal CmimiPaguar, decimal DiscountPercent = 0, string PaymentStatus = "Manual");

public record ProgramCreateDto(string EmriProgramit, string? Pershkrimi, int KohezgjatjaJave, string? Qellimi, string? Ushtrimet, string? Dieta);
public record ProgramUpdateDto(string EmriProgramit, string? Pershkrimi, int KohezgjatjaJave, string? Qellimi, string? Ushtrimet, string? Dieta);
public record ProgramResponseDto(int ProgramId, string EmriProgramit, string? Pershkrimi, int KohezgjatjaJave, string? Qellimi, string? Ushtrimet, string? Dieta);
public record KlientProgramCreateDto(int KlientId, int ProgramId, DateTime DataFillimit, DateTime? DataMbarimit, int Progresi, string Statusi);
public record KlientProgramUpdateDto(int KlientId, int ProgramId, DateTime DataFillimit, DateTime? DataMbarimit, int Progresi, string Statusi);
public record KlientProgramResponseDto(int KpId, int KlientId, string KlientEmri, int ProgramId, string ProgramEmri, DateTime DataFillimit, DateTime? DataMbarimit, int Progresi, string Statusi);

public record ProduktCreateDto(string EmriProduktit, string? Kategoria, string? Pershkrimi, decimal Cmimi, int SasiaStok, bool Aktiv = true);
public record ProduktUpdateDto(string EmriProduktit, string? Kategoria, string? Pershkrimi, decimal Cmimi, int SasiaStok, bool Aktiv);
public record ProduktResponseDto(int ProduktId, string EmriProduktit, string? Kategoria, string? Pershkrimi, decimal Cmimi, int SasiaStok, bool Aktiv);

public record ShitjeCreateDto(int KlientId, int ProduktId, int Sasia, decimal CmimiTotal, string TipiPageses = "Kesh", string StatusiPageses = "Paguar", string? KodiZbritjes = null, int PikatPerdorur = 0);
public record ShitjeUpdateDto(int KlientId, int ProduktId, int Sasia, decimal CmimiTotal, string TipiPageses = "Kesh", string StatusiPageses = "Paguar");
public record ShitjeResponseDto(int ShitjeId, int KlientId, string KlientEmri, int ProduktId, string ProduktEmri, int Sasia, decimal CmimiTotal, DateTime DataShitjes, string TipiPageses, string StatusiPageses);
public record UpdatePaymentStatusDto(string StatusiPageses);

public record VleresimCreateDto(int KlientId, int SherbimId, int TerapistId, int Nota, string? Komenti);
public record VleresimUpdateDto(int KlientId, int SherbimId, int TerapistId, int Nota, string? Komenti);
public record VleresimResponseDto(int VleresimId, int KlientId, string KlientEmri, int SherbimId, string SherbimEmri, int TerapistId, string TerapistEmri, int Nota, string? Komenti, DateTime DataVleresimit);

public record DashboardStatsDto(int TotalKlientet, int TotalTerminet, int TerminetSot, int AnetaresimiAktiv, decimal TeDheratMujore, int TerapistetAktiv, int ProductetNeStok, double NotaMesatare);

// ── Clinical notes ───────────────────────────────────────────────────────────
public record KlientShenimCreateDto(int KlientId, int? TerminId, int? TerapistId, string Tipi, string Permbajtja, bool Privat = false);
public record KlientShenimUpdateDto(string Tipi, string Permbajtja, bool Privat);
public record KlientShenimResponseDto(int ShenimId, int KlientId, string KlientEmri, int? TerminId, int? TerapistId, string? TerapistEmri, string Tipi, string Permbajtja, bool Privat, DateTime DataKrijimit);

// ── Body measurements ────────────────────────────────────────────────────────
public record KlientMatjeCreateDto(int KlientId, DateTime? DataMatjes, decimal? PeshaKg, decimal? GjatesiaCm, decimal? YndyraTrupore, decimal? BeliCm, decimal? KofshaCm, string? Shenim);
public record KlientMatjeUpdateDto(DateTime DataMatjes, decimal? PeshaKg, decimal? GjatesiaCm, decimal? YndyraTrupore, decimal? BeliCm, decimal? KofshaCm, string? Shenim);
public record KlientMatjeResponseDto(int MatjeId, int KlientId, DateTime DataMatjes, decimal? PeshaKg, decimal? GjatesiaCm, decimal? YndyraTrupore, decimal? BeliCm, decimal? KofshaCm, decimal? Bmi, string? Shenim);

// ── Loyalty points ───────────────────────────────────────────────────────────
public record KlientPikaCreateDto(int KlientId, int Pike, string Tipi, int? LidhjeId, string? Shenim);
public record KlientPikaResponseDto(int PikaId, int KlientId, int Pike, string Tipi, int? LidhjeId, string? Shenim, DateTime DataKrijimit);
public record KlientPikatBalanceDto(int KlientId, string KlientEmri, int Balanca, int FituarTotal, int ShperblerTotal);

// ── Recurring booking ────────────────────────────────────────────────────────
// dataFillimit = first session date; intervaliJave = e.g. 1 = weekly, 2 = bi-weekly;
// hereNumri = total number of sessions to create.
public record RecurringTerminCreateDto(
    int KlientId, int SherbimId, int TerapistId,
    DateTime DataFillimit, TimeSpan OraFillimit, TimeSpan OraMbarimit,
    int IntervaliJave, int HereNumri,
    string Statusi = "Planifikuar", string? Shenimet = null);

public record RecurringTerminResultDto(int Krijuar, int Anashkaluar, List<int> TerminIds, List<string> Mesazhet);

public record RescheduleProposalDto(DateTime ProposedStart, DateTime ProposedEnd, string? Note = null);
public record AppointmentQuoteDto(int SherbimId, decimal BasePrice, string LoyaltyTier, decimal DiscountPercent, decimal DiscountAmount, decimal FinalPrice);
public record MembershipQuoteDto(int PaketId, decimal BasePrice, string LoyaltyTier, decimal DiscountPercent, decimal DiscountAmount, decimal FinalPrice);
public record ConsentAcceptDto(string ConsentType = "PrivacyPolicy", string Version = "v1", bool Accepted = true);
public record ConsentLogResponseDto(int ConsentLogId, int? KlientId, string? UserId, string ConsentType, string Version, bool Accepted, DateTime CreatedAt);
public record TemplateUpsertDto(string Key, string Name, string Channel, string? Subject, string Body, bool Active = true);
public record TemplateResponseDto(int TemplateId, string Key, string Name, string Channel, string? Subject, string Body, bool Active, DateTime UpdatedAt);
public record LokacioniDto(string Emri, string? Adresa, string? Telefoni, bool Aktiv = true);
public record LokacioniResponseDto(int LokacioniId, string Emri, string? Adresa, string? Telefoni, bool Aktiv, DateTime CreatedAt);
public record NotificationResponseDto(int NotificationId, string Type, string Title, string Message, string? Link, bool IsRead, DateTime CreatedAt, DateTime? ReadAt);
public record TwoFactorVerifyDto(string Code);
public record StripeMembershipCheckoutDto(int PaketId);
