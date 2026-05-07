using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Models.Domain;
using WellnessAPI.Models.Identity;

namespace WellnessAPI.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Klient> Klientet => Set<Klient>();
    public DbSet<Sherbim> Sherbimet => Set<Sherbim>();
    public DbSet<Terapist> Terapistet => Set<Terapist>();
    public DbSet<Termin> Terminet => Set<Termin>();
    public DbSet<PaketaWellness> PaketaWellness => Set<PaketaWellness>();
    public DbSet<Anetaresim> Anetaresimet => Set<Anetaresim>();
    public DbSet<Models.Domain.Program> Programet => Set<Models.Domain.Program>();
    public DbSet<KlientProgram> KlientProgramet => Set<KlientProgram>();
    public DbSet<Produkt> Produktet => Set<Produkt>();
    public DbSet<ShitjeProdukteve> ShitjetProduktet => Set<ShitjeProdukteve>();
    public DbSet<Vleresim> Vlereisimet => Set<Vleresim>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PaketaSherbim> PaketaSherbimet => Set<PaketaSherbim>();
    public DbSet<TerapistAvailability> TerapistAvailability => Set<TerapistAvailability>();
    public DbSet<TerminStatusHistory> TerminStatusHistory => Set<TerminStatusHistory>();
    public DbSet<WaitingListEntry> WaitingList => Set<WaitingListEntry>();
    public DbSet<ClientDocument> ClientDocuments => Set<ClientDocument>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<BackupRecord> BackupRecords => Set<BackupRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // RefreshToken
        builder.Entity<RefreshToken>(e =>
        {
            e.Property(r => r.Token).HasMaxLength(500);
            e.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Klient
        builder.Entity<Klient>(e =>
        {
            e.HasKey(k => k.KlientId);
            e.HasIndex(k => k.Email).IsUnique();
        });

        // Sherbim
        builder.Entity<Sherbim>().HasKey(s => s.SherbimId);

        // Terapist
        builder.Entity<Terapist>(e =>
        {
            e.HasKey(t => t.TerapistId);
            e.HasIndex(t => t.Email).IsUnique();
        });

        // Termin
        builder.Entity<Termin>().HasKey(t => t.TerminId);

        // PaketaWellness
        builder.Entity<PaketaWellness>().HasKey(p => p.PaketId);

        // Anetaresim
        builder.Entity<Anetaresim>().HasKey(a => a.AnetaresimId);

        // Program
        builder.Entity<Models.Domain.Program>().HasKey(p => p.ProgramId);

        // Produkt
        builder.Entity<Produkt>().HasKey(p => p.ProduktId);

        // ShitjeProdukteve
        builder.Entity<ShitjeProdukteve>().HasKey(s => s.ShitjeId);

        // Vleresim
        builder.Entity<Vleresim>().HasKey(v => v.VleresimId);

        // Decimal columns
        builder.Entity<Sherbim>().Property(s => s.Cmimi).HasColumnType("decimal(10,2)");
        builder.Entity<PaketaWellness>().Property(p => p.Cmimi).HasColumnType("decimal(10,2)");
        builder.Entity<Anetaresim>().Property(a => a.CmimiPaguar).HasColumnType("decimal(10,2)");
        builder.Entity<Produkt>().Property(p => p.Cmimi).HasColumnType("decimal(10,2)");
        builder.Entity<ShitjeProdukteve>().Property(s => s.CmimiTotal).HasColumnType("decimal(10,2)");
        builder.Entity<Invoice>().Property(i => i.Amount).HasColumnType("decimal(10,2)");

        // FK Restrict to prevent cascade cycles
        builder.Entity<Termin>(e =>
        {
            e.HasOne(t => t.Klienti).WithMany(k => k.Terminet).HasForeignKey(t => t.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Sherbimi).WithMany(s => s.Terminet).HasForeignKey(t => t.SherbimId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Terapisti).WithMany(t => t.Terminet).HasForeignKey(t => t.TerapistId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Anetaresim>(e =>
        {
            e.HasOne(a => a.Klienti).WithMany(k => k.Anetaresimet).HasForeignKey(a => a.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Paketa).WithMany(p => p.Anetaresimet).HasForeignKey(a => a.PaketId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<KlientProgram>(e =>
        {
            e.HasKey(kp => kp.KpId);
            e.HasOne(kp => kp.Klienti).WithMany(k => k.KlientProgramet).HasForeignKey(kp => kp.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(kp => kp.Programi).WithMany(p => p.KlientProgramet).HasForeignKey(kp => kp.ProgramId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ShitjeProdukteve>(e =>
        {
            e.HasOne(s => s.Klienti).WithMany(k => k.ShitjetProduktet).HasForeignKey(s => s.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Produkti).WithMany(p => p.Shitjet).HasForeignKey(s => s.ProduktId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Vleresim>(e =>
        {
            e.HasOne(v => v.Klienti).WithMany(k => k.Vlereisimet).HasForeignKey(v => v.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Sherbimi).WithMany(s => s.Vlereisimet).HasForeignKey(v => v.SherbimId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Terapisti).WithMany(t => t.Vlereisimet).HasForeignKey(v => v.TerapistId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(v => new { v.KlientId, v.SherbimId }).IsUnique();
        });

        builder.Entity<PaketaSherbim>(e =>
        {
            e.HasKey(ps => ps.PaketaSherbimId);
            e.HasIndex(ps => new { ps.PaketId, ps.SherbimId }).IsUnique();
            e.HasOne(ps => ps.Paketa).WithMany(p => p.PaketaSherbimet).HasForeignKey(ps => ps.PaketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ps => ps.Sherbimi).WithMany(s => s.PaketaSherbimet).HasForeignKey(ps => ps.SherbimId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TerapistAvailability>(e =>
        {
            e.HasKey(a => a.TerapistAvailabilityId);
            e.HasOne(a => a.Terapisti).WithMany(t => t.Availability).HasForeignKey(a => a.TerapistId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TerminStatusHistory>(e =>
        {
            e.HasKey(h => h.TerminStatusHistoryId);
            e.HasOne(h => h.Termin).WithMany(t => t.StatusHistory).HasForeignKey(h => h.TerminId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WaitingListEntry>(e =>
        {
            e.HasKey(w => w.WaitingListEntryId);
            e.HasOne(w => w.Klienti).WithMany().HasForeignKey(w => w.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(w => w.Sherbimi).WithMany().HasForeignKey(w => w.SherbimId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(w => w.Terapisti).WithMany().HasForeignKey(w => w.TerapistId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ClientDocument>(e =>
        {
            e.HasKey(d => d.ClientDocumentId);
            e.HasOne(d => d.Klienti).WithMany().HasForeignKey(d => d.KlientId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Invoice>(e =>
        {
            e.HasKey(i => i.InvoiceId);
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
            e.HasOne(i => i.Shitje).WithMany().HasForeignKey(i => i.ShitjeId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Notification>().HasKey(n => n.NotificationId);
        builder.Entity<BackupRecord>().HasKey(b => b.BackupRecordId);

        // AuditLog
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.CreatedAt);
            e.HasIndex(a => a.UserId);
        });
    }
}
