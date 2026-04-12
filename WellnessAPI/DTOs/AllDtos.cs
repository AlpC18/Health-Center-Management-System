namespace WellnessAPI.DTOs;

// Auth DTOs
public record RegisterDto(string FirstName, string LastName, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string AccessToken, string RefreshToken, UserInfoDto User);
public record UserInfoDto(string Id, string Email, string FirstName, string LastName);
public record RefreshTokenRequestDto(string RefreshToken);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);

// Klient DTOs
public record KlientCreateDto(
    string Emri, string Mbiemri, string Email,
    string? Telefoni, DateTime? DataLindjes,
    string? Gjinia, string? KushtetShendetesore);

public record KlientUpdateDto(
    string Emri, string Mbiemri, string Email,
    string? Telefoni, DateTime? DataLindjes,
    string? Gjinia, string? KushtetShendetesore);

public record KlientResponseDto(
    int KlientId, string Emri, string Mbiemri, string Email,
    string? Telefoni, DateTime? DataLindjes, string? Gjinia,
    string? KushtetShendetesore, DateTime DataRegjistrimit);

// Sherbim DTOs
public record SherbimCreateDto(
    string EmriSherbimit, string? Kategoria, string? Pershkrimi,
    int KohezgjatjaMin, decimal Cmimi, bool Aktiv);

public record SherbimUpdateDto(
    string EmriSherbimit, string? Kategoria, string? Pershkrimi,
    int KohezgjatjaMin, decimal Cmimi, bool Aktiv);

public record SherbimResponseDto(
    int SherbimId, string EmriSherbimit, string? Kategoria,
    string? Pershkrimi, int KohezgjatjaMin, decimal Cmimi, bool Aktiv);

// Terapist DTOs
public record TerapistCreateDto(
    string Emri, string Mbiemri, string? Specializimi,
    string? Licenca, string Email, string? Telefoni, bool Aktiv);

public record TerapistUpdateDto(
    string Emri, string Mbiemri, string? Specializimi,
    string? Licenca, string Email, string? Telefoni, bool Aktiv);

public record TerapistResponseDto(
    int TerapistId, string Emri, string Mbiemri, string? Specializimi,
    string? Licenca, string Email, string? Telefoni, bool Aktiv);

// Termin DTOs
public record TerminCreateDto(
    int KlientId, int SherbimId, int TerapistId,
    DateTime DataTerminit, TimeSpan OraFillimit, TimeSpan OraMbarimit,
    string Statusi, string? Shenimet);

public record TerminUpdateDto(
    int KlientId, int SherbimId, int TerapistId,
    DateTime DataTerminit, TimeSpan OraFillimit, TimeSpan OraMbarimit,
    string Statusi, string? Shenimet);

public record TerminResponseDto(
    int TerminId, int KlientId, string KlientEmri,
    int SherbimId, string SherbimEmri,
    int TerapistId, string TerapistEmri,
    DateTime DataTerminit, TimeSpan OraFillimit, TimeSpan OraMbarimit,
    string Statusi, string? Shenimet);

// PaketaWellness DTOs
public record PaketaCreateDto(
    string EmriPaketes, string? Pershkrimi, string? SherbimiPerfshire,
    decimal Cmimi, int KohezgjatjaMuaj, bool Aktive);

public record PaketaUpdateDto(
    string EmriPaketes, string? Pershkrimi, string? SherbimiPerfshire,
    decimal Cmimi, int KohezgjatjaMuaj, bool Aktive);

public record PaketaResponseDto(
    int PaketId, string EmriPaketes, string? Pershkrimi,
    string? SherbimiPerfshire, decimal Cmimi, int KohezgjatjaMuaj, bool Aktive);

// Anetaresim DTOs
public record AnetaresimCreateDto(
    int KlientId, int PaketId, DateTime DataFillimit,
    DateTime DataMbarimit, string Statusi, decimal CmimiPaguar);

public record AnetaresimUpdateDto(
    int KlientId, int PaketId, DateTime DataFillimit,
    DateTime DataMbarimit, string Statusi, decimal CmimiPaguar);

public record AnetaresimResponseDto(
    int AnetaresimId, int KlientId, string KlientEmri,
    int PaketId, string PaketEmri,
    DateTime DataFillimit, DateTime DataMbarimit,
    string Statusi, decimal CmimiPaguar);

// Program DTOs
public record ProgramCreateDto(
    string EmriProgramit, string? Pershkrimi, int KohezgjatjaJave,
    string? Qellimi, string? Ushtrimet, string? Dieta);

public record ProgramUpdateDto(
    string EmriProgramit, string? Pershkrimi, int KohezgjatjaJave,
    string? Qellimi, string? Ushtrimet, string? Dieta);

public record ProgramResponseDto(
    int ProgramId, string EmriProgramit, string? Pershkrimi,
    int KohezgjatjaJave, string? Qellimi, string? Ushtrimet, string? Dieta);

// KlientProgram DTOs
public record KlientProgramCreateDto(
    int KlientId, int ProgramId, DateTime DataFillimit,
    DateTime? DataMbarimit, int Progresi, string Statusi);

public record KlientProgramUpdateDto(
    int KlientId, int ProgramId, DateTime DataFillimit,
    DateTime? DataMbarimit, int Progresi, string Statusi);

public record KlientProgramResponseDto(
    int KpId, int KlientId, string KlientEmri,
    int ProgramId, string ProgramEmri,
    DateTime DataFillimit, DateTime? DataMbarimit,
    int Progresi, string Statusi);

// Produkt DTOs
public record ProduktCreateDto(
    string EmriProduktit, string? Kategoria, string? Pershkrimi,
    decimal Cmimi, int SasiaStok, bool Aktiv);

public record ProduktUpdateDto(
    string EmriProduktit, string? Kategoria, string? Pershkrimi,
    decimal Cmimi, int SasiaStok, bool Aktiv);

public record ProduktResponseDto(
    int ProduktId, string EmriProduktit, string? Kategoria,
    string? Pershkrimi, decimal Cmimi, int SasiaStok, bool Aktiv);

// ShitjeProdukteve DTOs
public record ShitjeCreateDto(
    int KlientId, int ProduktId, int Sasia,
    decimal CmimiTotal, DateTime DataShitjes);

public record ShitjeUpdateDto(
    int KlientId, int ProduktId, int Sasia,
    decimal CmimiTotal, DateTime DataShitjes);

public record ShitjeResponseDto(
    int ShitjeId, int KlientId, string KlientEmri,
    int ProduktId, string ProduktEmri,
    int Sasia, decimal CmimiTotal, DateTime DataShitjes);

// Vleresim DTOs
public record VleresimCreateDto(
    int KlientId, int SherbimId, int TerapistId,
    int Nota, string? Komenti, DateTime DataVleresimit);

public record VleresimUpdateDto(
    int KlientId, int SherbimId, int TerapistId,
    int Nota, string? Komenti, DateTime DataVleresimit);

public record VleresimResponseDto(
    int VleresimId, int KlientId, string KlientEmri,
    int SherbimId, string SherbimEmri,
    int TerapistId, string TerapistEmri,
    int Nota, string? Komenti, DateTime DataVleresimit);

// Dashboard DTO
public record DashboardStatsDto(
    int TotalKlientet,
    int TotalTerminet,
    int TerminetSot,
    int AnetaresimiAktiv,
    decimal TeDheratMujore,
    int TerapistetAktiv,
    int ProductetNeStok,
    double NotaMesatare);
