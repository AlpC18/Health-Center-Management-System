namespace WellnessAPI.Models.Domain;

public enum AppointmentStatus
{
    Planifikuar,
    Konfirmuar,
    NdryshimPropozuar,
    Perfunduar,
    Anuluar
}

public enum TemplateChannel
{
    Email,
    Sms
}

public class Klient
{
    public int KlientId { get; set; }
    public string Emri { get; set; } = string.Empty;
    public string Mbiemri { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefoni { get; set; }
    public DateTime? DataLindjes { get; set; }
    public string? Gjinia { get; set; }
    public string? KushtetShendetesore { get; set; }
    public string? FotoPath { get; set; }
    public DateTime DataRegjistrimit { get; set; } = DateTime.UtcNow;
    public string LoyaltyTier { get; set; } = "Bronze";
    public decimal DiscountPercent { get; set; } = 0;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Anetaresim> Anetaresimet { get; set; } = new List<Anetaresim>();
    public ICollection<KlientProgram> KlientProgramet { get; set; } = new List<KlientProgram>();
    public ICollection<ShitjeProdukteve> ShitjetProduktet { get; set; } = new List<ShitjeProdukteve>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
    public ICollection<ConsentLog> ConsentLogs { get; set; } = new List<ConsentLog>();
}

public class Sherbim
{
    public int SherbimId { get; set; }
    public string EmriSherbimit { get; set; } = string.Empty;
    public string? Kategoria { get; set; }
    public string? Pershkrimi { get; set; }
    public int KohezgjatjaMin { get; set; }
    public decimal Cmimi { get; set; }
    public bool Aktiv { get; set; } = true;
    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
}

public class Terapist
{
    public int TerapistId { get; set; }
    public string? UserId { get; set; }
    public string Emri { get; set; } = string.Empty;
    public string Mbiemri { get; set; } = string.Empty;
    public string? Specializimi { get; set; }
    public string? Licenca { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefoni { get; set; }
    public bool Aktiv { get; set; } = true;
    public int? LokacioniId { get; set; }
    public Lokacioni? Lokacioni { get; set; }
    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
}

public class Termin
{
    public int TerminId { get; set; }
    public int KlientId { get; set; }
    public Klient Klienti { get; set; } = null!;
    public int SherbimId { get; set; }
    public Sherbim Sherbimi { get; set; } = null!;
    public int TerapistId { get; set; }
    public Terapist Terapisti { get; set; } = null!;
    public DateTime DataTerminit { get; set; }
    public TimeSpan OraFillimit { get; set; }
    public TimeSpan OraMbarimit { get; set; }
    public AppointmentStatus Statusi { get; set; } = AppointmentStatus.Planifikuar;
    public string? Shenimet { get; set; }
    public int? LokacioniId { get; set; }
    public Lokacioni? Lokacioni { get; set; }
    public DateTime? ProposedStart { get; set; }
    public DateTime? ProposedEnd { get; set; }
    public string? RescheduleProposedByUserId { get; set; }
    public string? RescheduleNote { get; set; }
    public DateTime? RescheduleProposedAt { get; set; }
    public DateTime? ReminderEmailSentAt { get; set; }
    public DateTime? ReminderSmsSentAt { get; set; }
}

public class PaketaWellness
{
    public int PaketId { get; set; }
    public string EmriPaketes { get; set; } = string.Empty;
    public string? Pershkrimi { get; set; }
    public string? SherbimiPerfshire { get; set; }
    public decimal Cmimi { get; set; }
    public int KohezgjatjaMuaj { get; set; }
    public bool Aktive { get; set; } = true;
    public ICollection<Anetaresim> Anetaresimet { get; set; } = new List<Anetaresim>();
}

public class Anetaresim
{
    public int AnetaresimId { get; set; }
    public int KlientId { get; set; }
    public Klient Klienti { get; set; } = null!;
    public int PaketId { get; set; }
    public PaketaWellness Paketa { get; set; } = null!;
    public DateTime DataFillimit { get; set; }
    public DateTime DataMbarimit { get; set; }
    public string Statusi { get; set; } = "Aktiv";
    public decimal CmimiPaguar { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public string? PaymentProvider { get; set; }
    public string? PaymentReference { get; set; }
    public string? StripeSessionId { get; set; }
    public string PaymentStatus { get; set; } = "Manual";
}

public class Lokacioni
{
    public int LokacioniId { get; set; }
    public string Emri { get; set; } = string.Empty;
    public string? Adresa { get; set; }
    public string? Telefoni { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Terapist> Terapistet { get; set; } = new List<Terapist>();
    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
}

public class ConsentLog
{
    public int ConsentLogId { get; set; }
    public int? KlientId { get; set; }
    public Klient? Klienti { get; set; }
    public string? UserId { get; set; }
    public string ConsentType { get; set; } = "PrivacyPolicy";
    public string Version { get; set; } = "v1";
    public bool Accepted { get; set; } = true;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Template
{
    public int TemplateId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TemplateChannel Channel { get; set; } = TemplateChannel.Email;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
}

public class Notification
{
    public int NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}

public class Program
{
    public int ProgramId { get; set; }
    public string EmriProgramit { get; set; } = string.Empty;
    public string? Pershkrimi { get; set; }
    public int KohezgjatjaJave { get; set; }
    public string? Qellimi { get; set; }
    public string? Ushtrimet { get; set; }
    public string? Dieta { get; set; }
    public ICollection<KlientProgram> KlientProgramet { get; set; } = new List<KlientProgram>();
}

public class KlientProgram
{
    public int KpId { get; set; }
    public int KlientId { get; set; }
    public Klient Klienti { get; set; } = null!;
    public int ProgramId { get; set; }
    public Program Programi { get; set; } = null!;
    public DateTime DataFillimit { get; set; }
    public DateTime? DataMbarimit { get; set; }
    public int Progresi { get; set; } = 0;
    public string Statusi { get; set; } = "Aktiv";
}

public class Produkt
{
    public int ProduktId { get; set; }
    public string EmriProduktit { get; set; } = string.Empty;
    public string? Kategoria { get; set; }
    public string? Pershkrimi { get; set; }
    public decimal Cmimi { get; set; }
    public int SasiaStok { get; set; } = 0;
    public bool Aktiv { get; set; } = true;
    public ICollection<ShitjeProdukteve> Shitjet { get; set; } = new List<ShitjeProdukteve>();
}

public class ShitjeProdukteve
{
    public int ShitjeId { get; set; }
    public int KlientId { get; set; }
    public Klient Klienti { get; set; } = null!;
    public int ProduktId { get; set; }
    public Produkt Produkti { get; set; } = null!;
    public int Sasia { get; set; }
    public decimal CmimiTotal { get; set; }
    public DateTime DataShitjes { get; set; } = DateTime.UtcNow;
    public string TipiPageses { get; set; } = "Kesh";
    public string StatusiPageses { get; set; } = "Paguar";
}

public class Vleresim
{
    public int VleresimId { get; set; }
    public int KlientId { get; set; }
    public Klient Klienti { get; set; } = null!;
    public int SherbimId { get; set; }
    public Sherbim Sherbimi { get; set; } = null!;
    public int TerapistId { get; set; }
    public Terapist Terapisti { get; set; } = null!;
    public int Nota { get; set; }
    public string? Komenti { get; set; }
    public DateTime DataVleresimit { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ── Additional CRUD entities ────────────────────────────────────────────────

public class Salla
{
    public int SallaId { get; set; }
    public string Emri { get; set; } = string.Empty;
    public int Kapaciteti { get; set; }
    public string? Tipi { get; set; }
    public string? Pershkrimi { get; set; }
    public bool Aktive { get; set; } = true;
}

public class Furnizuesi
{
    public int FurnizuesId { get; set; }
    public string Emri { get; set; } = string.Empty;
    public string? KontaktPersona { get; set; }
    public string? Email { get; set; }
    public string? Telefoni { get; set; }
    public string? Adresa { get; set; }
    public bool Aktiv { get; set; } = true;
    public DateTime DataRegjistrimit { get; set; } = DateTime.UtcNow;
}

public class Lajmerimi
{
    public int LajmerimId { get; set; }
    public string Titulli { get; set; } = string.Empty;
    public string Permbajtja { get; set; } = string.Empty;
    public string Audienca { get; set; } = "All";
    public string Prioriteti { get; set; } = "Mesem";
    public DateTime DataKrijimit { get; set; } = DateTime.UtcNow;
    public DateTime? DataSkadimit { get; set; }
    public bool Aktiv { get; set; } = true;
}

public class Zbritja
{
    public int ZbritjeId { get; set; }
    public string Kodi { get; set; } = string.Empty;
    public decimal PerqindjaZbritjes { get; set; }
    public DateTime DataFillimit { get; set; } = DateTime.UtcNow;
    public DateTime DataMbarimit { get; set; }
    public int LimitiPerdorimit { get; set; } = 100;
    public int HereshShfrytezuar { get; set; } = 0;
    public bool Aktive { get; set; } = true;
}

public class Pushimi
{
    public int PushimId { get; set; }
    public int TerapistId { get; set; }
    public DateTime DataFillimit { get; set; }
    public DateTime DataMbarimit { get; set; }
    public string? Arsyeja { get; set; }
    public string Statusi { get; set; } = "Kerkuar";
    public DateTime DataKerkimit { get; set; } = DateTime.UtcNow;
}

// ── Clinical notes (anamnese/treatment log) — per-visit history ──────────────
// One Klient -> many KlientShenime; optionally linked to a Termin.
public class KlientShenim
{
    public int ShenimId { get; set; }
    public int KlientId { get; set; }
    public int? TerminId { get; set; }              // optional: tied to a specific visit
    public int? TerapistId { get; set; }            // author (nullable for admin notes)
    public string Tipi { get; set; } = "Vezhgim";   // Anamnese | Trajtim | Vezhgim | Plan | Tjeter
    public string Permbajtja { get; set; } = string.Empty;
    public bool Privat { get; set; } = false;       // true = therapist+admin only
    public DateTime DataKrijimit { get; set; } = DateTime.UtcNow;
}

// ── Body measurements / progress tracking ────────────────────────────────────
public class KlientMatje
{
    public int MatjeId { get; set; }
    public int KlientId { get; set; }
    public DateTime DataMatjes { get; set; } = DateTime.UtcNow;
    public decimal? PeshaKg { get; set; }
    public decimal? GjatesiaCm { get; set; }
    public decimal? YndyraTrupore { get; set; }     // body fat %
    public decimal? BeliCm { get; set; }            // waist
    public decimal? KofshaCm { get; set; }          // hip
    public string? Shenim { get; set; }
}

// ── Loyalty ledger ───────────────────────────────────────────────────────────
// Positive Pike = earned; negative Pike = redeemed.
// Tipi: "ShitjeBlerje" | "Termin" | "Shperblim" | "Tjeter"
// LidhjeId optionally references the originating sale or appointment.
public class KlientPika
{
    public int PikaId { get; set; }
    public int KlientId { get; set; }
    public int Pike { get; set; }
    public string Tipi { get; set; } = "Tjeter";
    public int? LidhjeId { get; set; }
    public string? Shenim { get; set; }
    public DateTime DataKrijimit { get; set; } = DateTime.UtcNow;
}
