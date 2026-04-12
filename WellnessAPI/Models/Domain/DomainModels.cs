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

    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
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

    public ICollection<Termin> Terminet { get; set; } = new List<Termin>();
    public ICollection<Vleresim> Vlereisimet { get; set; } = new List<Vleresim>();
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

    public Klient Klienti { get; set; } = null!;
    public Sherbim Sherbimi { get; set; } = null!;
    public Terapist Terapisti { get; set; } = null!;
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
    public int PaketId { get; set; }
    public DateTime DataFillimit { get; set; }
    public DateTime DataMbarimit { get; set; }
    public string Statusi { get; set; } = "Aktiv";
    public decimal CmimiPaguar { get; set; }

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
    public bool Aktiv { get; set; } = true;

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
    public DateTime DataVleresimit { get; set; } = DateTime.UtcNow;

    public Klient Klienti { get; set; } = null!;
    public Sherbim Sherbimi { get; set; } = null!;
    public Terapist Terapisti { get; set; } = null!;
}
