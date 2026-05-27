using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Models.Domain;
using WellnessAPI.Models.Identity;

namespace WellnessAPI.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
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

    // Additional CRUD entities
    public DbSet<Salla> Sallat => Set<Salla>();
    public DbSet<Furnizuesi> Furnizuesit => Set<Furnizuesi>();
    public DbSet<Lajmerimi> Lajmerimet => Set<Lajmerimi>();
    public DbSet<Zbritja> Zbritjet => Set<Zbritja>();
    public DbSet<Pushimi> Pushimet => Set<Pushimi>();
    public DbSet<KlientShenim> KlientShenime => Set<KlientShenim>();
    public DbSet<KlientMatje> KlientMatjet => Set<KlientMatje>();
    public DbSet<KlientPika> KlientPikat => Set<KlientPika>();
    public DbSet<Lokacioni> Lokacionet => Set<Lokacioni>();
    public DbSet<ConsentLog> ConsentLogs => Set<ConsentLog>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(e => {
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).IsRequired().HasMaxLength(500);
            e.HasOne(r => r.User).WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PasswordResetToken>(e => {
            e.HasKey(r => r.Id);
            e.Property(r => r.TokenHash).IsRequired().HasMaxLength(128);
            e.HasIndex(r => r.TokenHash).IsUnique();
            e.HasIndex(r => new { r.UserId, r.UsedAt, r.ExpiresAt });
            e.HasOne(r => r.User).WithMany(u => u.PasswordResetTokens)
             .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationUser>(e => {
            e.Property(u => u.SmsOptIn).HasDefaultValue(true);
        });

        builder.Entity<Klient>(e => {
            e.HasKey(k => k.KlientId);
            e.Property(k => k.Emri).IsRequired().HasMaxLength(100);
            e.Property(k => k.Mbiemri).IsRequired().HasMaxLength(100);
            e.Property(k => k.Email).IsRequired().HasMaxLength(200);
            e.Property(k => k.LoyaltyTier).IsRequired().HasMaxLength(50).HasDefaultValue("Bronze");
            e.Property(k => k.DiscountPercent).HasColumnType("decimal(5,2)");
            e.HasIndex(k => k.Email).IsUnique();
        });

        builder.Entity<Sherbim>(e => {
            e.HasKey(s => s.SherbimId);
            e.Property(s => s.EmriSherbimit).IsRequired().HasMaxLength(200);
            e.Property(s => s.Cmimi).HasColumnType("decimal(10,2)");
        });

        builder.Entity<Terapist>(e => {
            e.HasKey(t => t.TerapistId);
            e.Property(t => t.Emri).IsRequired().HasMaxLength(100);
            e.Property(t => t.Mbiemri).IsRequired().HasMaxLength(100);
            e.Property(t => t.Email).IsRequired().HasMaxLength(200);
            e.Property(t => t.UserId).HasMaxLength(450);
            e.HasIndex(t => t.Email).IsUnique();
            e.HasIndex(t => t.UserId).IsUnique();
            e.HasOne(t => t.Lokacioni).WithMany(l => l.Terapistet)
             .HasForeignKey(t => t.LokacioniId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Termin>(e => {
            e.HasKey(t => t.TerminId);
            e.Property(t => t.Statusi).HasConversion<string>().HasMaxLength(40);
            e.Property(t => t.RescheduleProposedByUserId).HasMaxLength(450);
            e.HasOne(t => t.Klienti).WithMany(k => k.Terminet)
             .HasForeignKey(t => t.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Sherbimi).WithMany(s => s.Terminet)
             .HasForeignKey(t => t.SherbimId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Terapisti).WithMany(t => t.Terminet)
             .HasForeignKey(t => t.TerapistId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Lokacioni).WithMany(l => l.Terminet)
             .HasForeignKey(t => t.LokacioniId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<PaketaWellness>(e => {
            e.HasKey(p => p.PaketId);
            e.Property(p => p.EmriPaketes).IsRequired().HasMaxLength(200);
            e.Property(p => p.Cmimi).HasColumnType("decimal(10,2)");
        });

        builder.Entity<Anetaresim>(e => {
            e.HasKey(a => a.AnetaresimId);
            e.HasOne(a => a.Klienti).WithMany(k => k.Anetaresimet)
             .HasForeignKey(a => a.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Paketa).WithMany(p => p.Anetaresimet)
             .HasForeignKey(a => a.PaketId).OnDelete(DeleteBehavior.Restrict);
            e.Property(a => a.CmimiPaguar).HasColumnType("decimal(10,2)");
            e.Property(a => a.DiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(a => a.PaymentStatus).IsRequired().HasMaxLength(40).HasDefaultValue("Manual");
            e.HasIndex(a => a.StripeSessionId);
        });

        builder.Entity<Lokacioni>(e => {
            e.HasKey(l => l.LokacioniId);
            e.Property(l => l.Emri).IsRequired().HasMaxLength(150);
            e.Property(l => l.Adresa).HasMaxLength(250);
            e.Property(l => l.Telefoni).HasMaxLength(50);
            e.HasIndex(l => l.Emri).IsUnique();
        });

        builder.Entity<ConsentLog>(e => {
            e.HasKey(c => c.ConsentLogId);
            e.Property(c => c.ConsentType).IsRequired().HasMaxLength(100);
            e.Property(c => c.Version).IsRequired().HasMaxLength(50);
            e.Property(c => c.UserId).HasMaxLength(450);
            e.HasIndex(c => new { c.UserId, c.ConsentType, c.Version });
            e.HasIndex(c => new { c.KlientId, c.ConsentType, c.Version });
            e.HasOne(c => c.Klienti).WithMany(k => k.ConsentLogs)
             .HasForeignKey(c => c.KlientId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Template>(e => {
            e.HasKey(t => t.TemplateId);
            e.Property(t => t.Key).IsRequired().HasMaxLength(120);
            e.Property(t => t.Name).IsRequired().HasMaxLength(160);
            e.Property(t => t.Channel).HasConversion<string>().HasMaxLength(20);
            e.Property(t => t.Subject).HasMaxLength(250);
            e.Property(t => t.Body).IsRequired();
            e.HasIndex(t => new { t.Key, t.Channel }).IsUnique();
        });

        builder.Entity<Notification>(e => {
            e.HasKey(n => n.NotificationId);
            e.Property(n => n.UserId).IsRequired().HasMaxLength(450);
            e.Property(n => n.Type).IsRequired().HasMaxLength(60);
            e.Property(n => n.Title).IsRequired().HasMaxLength(200);
            e.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            e.Property(n => n.Link).HasMaxLength(300);
            e.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
        });

        builder.Entity<Models.Domain.Program>(e => {
            e.HasKey(p => p.ProgramId);
            e.Property(p => p.EmriProgramit).IsRequired().HasMaxLength(200);
        });

        builder.Entity<KlientProgram>(e => {
            e.HasKey(kp => kp.KpId);
            e.HasOne(kp => kp.Klienti).WithMany(k => k.KlientProgramet)
             .HasForeignKey(kp => kp.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(kp => kp.Programi).WithMany(p => p.KlientProgramet)
             .HasForeignKey(kp => kp.ProgramId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Produkt>(e => {
            e.HasKey(p => p.ProduktId);
            e.Property(p => p.EmriProduktit).IsRequired().HasMaxLength(200);
            e.Property(p => p.Cmimi).HasColumnType("decimal(10,2)");
        });

        builder.Entity<ShitjeProdukteve>(e => {
            e.HasKey(s => s.ShitjeId);
            e.HasOne(s => s.Klienti).WithMany(k => k.ShitjetProduktet)
             .HasForeignKey(s => s.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Produkti).WithMany(p => p.Shitjet)
             .HasForeignKey(s => s.ProduktId).OnDelete(DeleteBehavior.Restrict);
            e.Property(s => s.CmimiTotal).HasColumnType("decimal(10,2)");
        });

        builder.Entity<Vleresim>(e => {
            e.HasKey(v => v.VleresimId);
            e.HasOne(v => v.Klienti).WithMany(k => k.Vlereisimet)
             .HasForeignKey(v => v.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Sherbimi).WithMany(s => s.Vlereisimet)
             .HasForeignKey(v => v.SherbimId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Terapisti).WithMany(t => t.Vlereisimet)
             .HasForeignKey(v => v.TerapistId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(v => new { v.KlientId, v.SherbimId }).IsUnique();
        });

        builder.Entity<AuditLog>(e => {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).IsRequired().HasMaxLength(50);
            e.Property(a => a.Entity).IsRequired().HasMaxLength(100);
            e.HasIndex(a => a.CreatedAt);
            e.HasIndex(a => a.UserId);
        });

        builder.Entity<Salla>(e => {
            e.HasKey(s => s.SallaId);
            e.Property(s => s.Emri).IsRequired().HasMaxLength(150);
            e.HasIndex(s => s.Emri).IsUnique();
        });

        builder.Entity<Furnizuesi>(e => {
            e.HasKey(f => f.FurnizuesId);
            e.Property(f => f.Emri).IsRequired().HasMaxLength(200);
            e.HasIndex(f => f.Emri);
        });

        builder.Entity<Lajmerimi>(e => {
            e.HasKey(l => l.LajmerimId);
            e.Property(l => l.Titulli).IsRequired().HasMaxLength(250);
            e.Property(l => l.Permbajtja).IsRequired();
            e.HasIndex(l => l.DataKrijimit);
        });

        builder.Entity<Zbritja>(e => {
            e.HasKey(z => z.ZbritjeId);
            e.Property(z => z.Kodi).IsRequired().HasMaxLength(50);
            e.Property(z => z.PerqindjaZbritjes).HasColumnType("decimal(5,2)");
            e.HasIndex(z => z.Kodi).IsUnique();
        });

        builder.Entity<Pushimi>(e => {
            e.HasKey(p => p.PushimId);
            e.HasIndex(p => p.TerapistId);
            e.HasIndex(p => p.Statusi);
        });

        builder.Entity<KlientShenim>(e => {
            e.HasKey(s => s.ShenimId);
            e.Property(s => s.Tipi).IsRequired().HasMaxLength(20);
            e.Property(s => s.Permbajtja).IsRequired();
            e.HasIndex(s => s.KlientId);
            e.HasIndex(s => s.TerminId);
            e.HasIndex(s => s.DataKrijimit);
        });

        builder.Entity<KlientMatje>(e => {
            e.HasKey(m => m.MatjeId);
            e.Property(m => m.PeshaKg).HasColumnType("decimal(6,2)");
            e.Property(m => m.GjatesiaCm).HasColumnType("decimal(6,2)");
            e.Property(m => m.YndyraTrupore).HasColumnType("decimal(5,2)");
            e.Property(m => m.BeliCm).HasColumnType("decimal(6,2)");
            e.Property(m => m.KofshaCm).HasColumnType("decimal(6,2)");
            e.HasIndex(m => new { m.KlientId, m.DataMatjes });
        });

        builder.Entity<KlientPika>(e => {
            e.HasKey(p => p.PikaId);
            e.Property(p => p.Tipi).IsRequired().HasMaxLength(20);
            e.HasIndex(p => p.KlientId);
            e.HasIndex(p => p.DataKrijimit);
        });
    }
}
