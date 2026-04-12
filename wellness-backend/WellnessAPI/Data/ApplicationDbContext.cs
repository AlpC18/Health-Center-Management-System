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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(e => {
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).IsRequired().HasMaxLength(500);
            e.HasOne(r => r.User).WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Klient>(e => {
            e.HasKey(k => k.KlientId);
            e.Property(k => k.Emri).IsRequired().HasMaxLength(100);
            e.Property(k => k.Mbiemri).IsRequired().HasMaxLength(100);
            e.Property(k => k.Email).IsRequired().HasMaxLength(200);
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
            e.HasIndex(t => t.Email).IsUnique();
        });

        builder.Entity<Termin>(e => {
            e.HasKey(t => t.TerminId);
            e.HasOne(t => t.Klienti).WithMany(k => k.Terminet)
             .HasForeignKey(t => t.KlientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Sherbimi).WithMany(s => s.Terminet)
             .HasForeignKey(t => t.SherbimId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Terapisti).WithMany(t => t.Terminet)
             .HasForeignKey(t => t.TerapistId).OnDelete(DeleteBehavior.Restrict);
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
        });

        builder.Entity<AuditLog>(e => {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).IsRequired().HasMaxLength(50);
            e.Property(a => a.Entity).IsRequired().HasMaxLength(100);
            e.HasIndex(a => a.CreatedAt);
            e.HasIndex(a => a.UserId);
        });
    }
}
