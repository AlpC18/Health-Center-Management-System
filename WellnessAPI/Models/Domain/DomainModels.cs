namespace WellnessAPI.Models.Domain;

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
    public string? Alergjite { get; set; }
    public string? ShenimeMjekesore { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime DataRegjistrimit { get; set; } = DateTime.UtcNow;

    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Anetaresim> Anetaresimet { get; set; } = new List<Anetaresim>();
    public ICollection<KlientProgram> KlientProgramet { get; set; } = new List<KlientProgram>();
    public ICollection<ShitjeProdukteve> ShitjetProduktet { get; set; } = new List<ShitjeProdukteve>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
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
    public bool IsDeleted { get; set; }

    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
    public ICollection<PaketaSherbim> PaketaSherbimet { get; set; } = new List<PaketaSherbim>();
}

public class Terapist
{
    public int TerapistId { get; set; }
    public string Emri { get; set; } = string.Empty;
    public string Mbiemri { get; set; } = string.Empty;
    public string? Specializimi { get; set; }
    public string? Licenca { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefoni { get; set; }
    public bool Aktiv { get; set; } = true;
    public TimeSpan OrariFillimit { get; set; } = new(9, 0, 0);
    public TimeSpan OrariMbarimit { get; set; } = new(17, 0, 0);
    public bool IsDeleted { get; set; }

    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
    public ICollection<TerapistAvailability> Availability { get; set; } = new List<TerapistAvailability>();
}

public class Termin
{
    public int TerminId { get; set; }
    public int KlientId { get; set; }
    public int SherbimId { get; set; }
    public int TerapistId { get; set; }
    public DateTime DataTerminit { get; set; }
    public TimeSpan OraFillimit { get; set; }
    public TimeSpan OraMbarimit { get; set; }
    public string Statusi { get; set; } = "Planifikuar";
    public string? Shenimet { get; set; }
    public string? ArsyeAnulimi { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }

    public Klient Klienti { get; set; } = null!;
    public Sherbim Sherbimi { get; set; } = null!;
    public Terapist Terapisti { get; set; } = null!;
    public ICollection<TerminStatusHistory> StatusHistory { get; set; } = new List<TerminStatusHistory>();
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
    public bool IsDeleted { get; set; }

    public ICollection<Anetaresim> Anetaresimet { get; set; } = new List<Anetaresim>();
    public ICollection<PaketaSherbim> PaketaSherbimet { get; set; } = new List<PaketaSherbim>();
}

public class Anetaresim
{
    public int AnetaresimId { get; set; }
    public int KlientId { get; set; }
    public int PaketId { get; set; }
    public DateTime DataFillimit { get; set; }
    public DateTime DataMbarimit { get; set; }
    public string Statusi { get; set; } = "Aktiv";
    public decimal CmimiPaguar { get; set; }
    public bool ExpiryReminderSent { get; set; }

    public Klient Klienti { get; set; } = null!;
    public PaketaWellness Paketa { get; set; } = null!;
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
    public int ProgramId { get; set; }
    public DateTime DataFillimit { get; set; }
    public DateTime? DataMbarimit { get; set; }
    public int Progresi { get; set; } = 0;
    public string Statusi { get; set; } = "Aktiv";

    public Klient Klienti { get; set; } = null!;
    public Program Programi { get; set; } = null!;
}

public class Produkt
{
    public int ProduktId { get; set; }
    public string EmriProduktit { get; set; } = string.Empty;
    public string? Kategoria { get; set; }
    public string? Pershkrimi { get; set; }
    public decimal Cmimi { get; set; }
    public int SasiaStok { get; set; }
    public int MinimumStock { get; set; } = 5;
    public bool Aktiv { get; set; } = true;
    public bool IsDeleted { get; set; }

    public ICollection<ShitjeProdukteve> Shitjet { get; set; } = new List<ShitjeProdukteve>();
}

public class ShitjeProdukteve
{
    public int ShitjeId { get; set; }
    public int KlientId { get; set; }
    public int ProduktId { get; set; }
    public int Sasia { get; set; }
    public decimal CmimiTotal { get; set; }
    public DateTime DataShitjes { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "Cash";
    public string PaymentStatus { get; set; } = "Paid";

    public Klient Klienti { get; set; } = null!;
    public Produkt Produkti { get; set; } = null!;
}

public class Vleresim
{
    public int VleresimId { get; set; }
    public int KlientId { get; set; }
    public int SherbimId { get; set; }
    public int TerapistId { get; set; }
    public int Nota { get; set; }
    public string? Komenti { get; set; }
    public string ModerationStatus { get; set; } = "Pending";
    public DateTime DataVleresimit { get; set; } = DateTime.UtcNow;

    public Klient Klienti { get; set; } = null!;
    public Sherbim Sherbimi { get; set; } = null!;
    public Terapist Terapisti { get; set; } = null!;
}

public class PaketaSherbim
{
    public int PaketaSherbimId { get; set; }
    public int PaketId { get; set; }
    public int SherbimId { get; set; }
    public int SeancaPerfshire { get; set; } = 1;

    public PaketaWellness Paketa { get; set; } = null!;
    public Sherbim Sherbimi { get; set; } = null!;
}

public class TerapistAvailability
{
    public int TerapistAvailabilityId { get; set; }
    public int TerapistId { get; set; }
    public DayOfWeek DitaJaves { get; set; }
    public TimeSpan OraFillimit { get; set; }
    public TimeSpan OraMbarimit { get; set; }
    public bool Aktiv { get; set; } = true;

    public Terapist Terapisti { get; set; } = null!;
}

public class TerminStatusHistory
{
    public int TerminStatusHistoryId { get; set; }
    public int TerminId { get; set; }
    public string StatusiNga { get; set; } = string.Empty;
    public string StatusiNe { get; set; } = string.Empty;
    public string? Arsyeja { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public Termin Termin { get; set; } = null!;
}

public class WaitingListEntry
{
    public int WaitingListEntryId { get; set; }
    public int KlientId { get; set; }
    public int SherbimId { get; set; }
    public int? TerapistId { get; set; }
    public DateTime PreferredDate { get; set; }
    public string Statusi { get; set; } = "Waiting";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Klient Klienti { get; set; } = null!;
    public Sherbim Sherbimi { get; set; } = null!;
    public Terapist? Terapisti { get; set; }
}

public class ClientDocument
{
    public int ClientDocumentId { get; set; }
    public int KlientId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Klient Klienti { get; set; } = null!;
}

public class Invoice
{
    public int InvoiceId { get; set; }
    public int ShitjeId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string Statusi { get; set; } = "Issued";

    public ShitjeProdukteve Shitje { get; set; } = null!;
}

public class Notification
{
    public int NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BackupRecord
{
    public int BackupRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Statusi { get; set; } = "Created";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
