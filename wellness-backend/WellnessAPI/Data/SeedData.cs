using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Models.Domain;
using WellnessAPI.Models.Identity;

namespace WellnessAPI.Data;

public static class SeedData
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. ROLES
        string[] roleNames = { "Admin", "Therapist", "Klient" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. SYSTEM USERS (always ensure test logins exist)
        await EnsureUserWithRoleAsync(userManager, "admin@wellness.com", "Admin123!", "Admin", "Wellness", "Admin");
        await EnsureUserWithRoleAsync(userManager, "therapist@wellness.com", "Therapist123!", "Arta", "Krasniqi", "Therapist");

        if (db.Klientet.Any())
        {
            var klient = await db.Klientet.AsNoTracking().OrderBy(k => k.KlientId).FirstOrDefaultAsync();
            await EnsureUserWithRoleAsync(
                userManager,
                "client@wellness.com",
                "Client123!",
                klient?.Emri ?? "Client",
                klient?.Mbiemri ?? "Wellness",
                "Klient",
                klient?.KlientId.ToString());
            return;
        }

        // 3. TERAPISTET
        var terapistet = new List<Terapist>
        {
            new() { Emri="Arta", Mbiemri="Krasniqi", Specializimi="Masazh Terapeutik", Licenca="LIC-2023-001", Email="arta.k@wellness.com", Telefoni="+383 44 111 222", Aktiv=true },
            new() { Emri="Blerim", Mbiemri="Hoxha", Specializimi="Yoga & Meditim", Licenca="LIC-2023-002", Email="blerim.h@wellness.com", Telefoni="+383 44 333 444", Aktiv=true },
            new() { Emri="Drita", Mbiemri="Berisha", Specializimi="Fizioterapi", Licenca="LIC-2023-003", Email="drita.b@wellness.com", Telefoni="+383 44 555 666", Aktiv=true },
            new() { Emri="Fitim", Mbiemri="Gashi", Specializimi="Nutricion", Licenca="LIC-2023-004", Email="fitim.g@wellness.com", Telefoni="+383 44 777 888", Aktiv=true },
            new() { Emri="Lirie", Mbiemri="Morina", Specializimi="Spa & Beauty", Licenca="LIC-2023-005", Email="lirie.m@wellness.com", Telefoni="+383 44 999 000", Aktiv=true },
        };
        db.Terapistet.AddRange(terapistet);

        // 4. SHERBIMET
        var sherbimet = new List<Sherbim>
        {
            new() { EmriSherbimit="Masazh Relaksues", Kategoria="Masazh", Pershkrimi="Masazh i plotë trupor", KohezgjatjaMin=60, Cmimi=35, Aktiv=true },
            new() { EmriSherbimit="Masazh Terapeutik", Kategoria="Masazh", Pershkrimi="Masazh për dhimbjet", KohezgjatjaMin=90, Cmimi=50, Aktiv=true },
            new() { EmriSherbimit="Yoga Fillestarë", Kategoria="Yoga", Pershkrimi="Klasa bazike", KohezgjatjaMin=60, Cmimi=20, Aktiv=true },
            new() { EmriSherbimit="Yoga Avancuar", Kategoria="Yoga", Pershkrimi="Klasa avancuar", KohezgjatjaMin=75, Cmimi=25, Aktiv=true },
            new() { EmriSherbimit="Trajtim Spa", Kategoria="Spa", Pershkrimi="Trajtim i plotë spa", KohezgjatjaMin=120, Cmimi=80, Aktiv=true },
            new() { EmriSherbimit="Fizioterapi", Kategoria="Fizioterapi", Pershkrimi="Seancë fizioterapie", KohezgjatjaMin=45, Cmimi=40, Aktiv=true },
            new() { EmriSherbimit="Konsultë Nutricion", Kategoria="Nutricion", Pershkrimi="Konsultë dietë", KohezgjatjaMin=30, Cmimi=30, Aktiv=true },
            new() { EmriSherbimit="Meditim i Drejtuar", Kategoria="Meditim", Pershkrimi="Seancë meditimi", KohezgjatjaMin=45, Cmimi=15, Aktiv=true },
        };
        db.Sherbimet.AddRange(sherbimet);

        // 5. KLIENTET
        var klientet = new List<Klient>
        {
            new() { Emri="Alban", Mbiemri="Mustafa", Email="alban.m@email.com", Telefoni="+383 45 100 001", DataLindjes=new DateTime(1990,3,15), Gjinia="M", DataRegjistrimit=DateTime.UtcNow.AddMonths(-6) },
            new() { Emri="Besa", Mbiemri="Ahmeti", Email="besa.a@email.com", Telefoni="+383 45 100 002", DataLindjes=new DateTime(1985,7,22), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddMonths(-5) },
            new() { Emri="Cunatë", Mbiemri="Shala", Email="cunate.sh@email.com", Telefoni="+383 45 100 003", DataLindjes=new DateTime(1992,11,8), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddMonths(-5) },
            new() { Emri="Donat", Mbiemri="Gashi", Email="donat.g@email.com", Telefoni="+383 45 100 004", DataLindjes=new DateTime(1988,1,30), Gjinia="M", DataRegjistrimit=DateTime.UtcNow.AddMonths(-4) },
            new() { Emri="Elona", Mbiemri="Kelmendi", Email="elona.k@email.com", Telefoni="+383 45 100 005", DataLindjes=new DateTime(1995,5,14), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddMonths(-4) },
            new() { Emri="Fisnik", Mbiemri="Berisha", Email="fisnik.b@email.com", Telefoni="+383 45 100 006", DataLindjes=new DateTime(1983,9,3), Gjinia="M", DataRegjistrimit=DateTime.UtcNow.AddMonths(-3) },
            new() { Emri="Genta", Mbiemri="Hoxha", Email="genta.h@email.com", Telefoni="+383 45 100 007", DataLindjes=new DateTime(1991,12,19), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddMonths(-3) },
            new() { Emri="Hekuran", Mbiemri="Lushi", Email="hekuran.l@email.com", Telefoni="+383 45 100 008", DataLindjes=new DateTime(1987,4,25), Gjinia="M", DataRegjistrimit=DateTime.UtcNow.AddMonths(-2) },
            new() { Emri="Igballe", Mbiemri="Rama", Email="igballe.r@email.com", Telefoni="+383 45 100 009", DataLindjes=new DateTime(1993,8,11), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddMonths(-2) },
            new() { Emri="Jeton", Mbiemri="Morina", Email="jeton.m@email.com", Telefoni="+383 45 100 010", DataLindjes=new DateTime(1989,6,7), Gjinia="M", DataRegjistrimit=DateTime.UtcNow.AddMonths(-1) },
            new() { Emri="Kaltrina", Mbiemri="Vula", Email="kaltrina.v@email.com", Telefoni="+383 45 100 011", DataLindjes=new DateTime(1996,2,28), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddMonths(-1) },
            new() { Emri="Labinot", Mbiemri="Kastrati", Email="labinot.k@email.com", Telefoni="+383 45 100 012", DataLindjes=new DateTime(1984,10,16), Gjinia="M", DataRegjistrimit=DateTime.UtcNow.AddDays(-20) },
            new() { Emri="Mimoza", Mbiemri="Bytyqi", Email="mimoza.b@email.com", Telefoni="+383 45 100 013", DataLindjes=new DateTime(1994,3,5), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddDays(-15) },
            new() { Emri="Nderim", Mbiemri="Syla", Email="nderim.s@email.com", Telefoni="+383 45 100 014", DataLindjes=new DateTime(1986,7,9), Gjinia="M", DataRegjistrimit=DateTime.UtcNow.AddDays(-10) },
            new() { Emri="Orgesa", Mbiemri="Hyseni", Email="orgesa.h@email.com", Telefoni="+383 45 100 015", DataLindjes=new DateTime(1997,1,21), Gjinia="F", DataRegjistrimit=DateTime.UtcNow.AddDays(-5) },
        };
        db.Klientet.AddRange(klientet);

        // 6. PAKETAT
        var paketat = new List<PaketaWellness>
        {
            new() { EmriPaketes="Paketa Bazike", Pershkrimi="Shërbime bazike", SherbimiPerfshire="Masazh, Yoga", Cmimi=99, KohezgjatjaMuaj=1, Aktive=true },
            new() { EmriPaketes="Paketa Premium", Pershkrimi="Qasje e plotë", SherbimiPerfshire="Të gjitha shërbimet", Cmimi=199, KohezgjatjaMuaj=1, Aktive=true },
            new() { EmriPaketes="Paketa Familjare", Pershkrimi="Për 2 persona", SherbimiPerfshire="Masazh, Yoga, Meditim", Cmimi=149, KohezgjatjaMuaj=1, Aktive=true },
            new() { EmriPaketes="Paketa Vjetore", Pershkrimi="Kursim 12 muaj", SherbimiPerfshire="Të gjitha shërbimet", Cmimi=999, KohezgjatjaMuaj=12, Aktive=true },
        };
        db.PaketaWellness.AddRange(paketat);

        // 7. PRODUKTET
        var produktet = new List<Produkt>
        {
            new() { EmriProduktit="Vaj Lavandule", Kategoria="Vajra & Kreme", Pershkrimi="Vaj esencial 30ml", Cmimi=12, SasiaStok=50, Aktiv=true },
            new() { EmriProduktit="Krem Masazhi", Kategoria="Vajra & Kreme", Pershkrimi="Krem profesional 200ml", Cmimi=18, SasiaStok=30, Aktiv=true },
            new() { EmriProduktit="Vitamina C", Kategoria="Suplemente", Pershkrimi="1000mg, 60 tableta", Cmimi=15, SasiaStok=100, Aktiv=true },
            new() { EmriProduktit="Tapete Yoga", Kategoria="Pajisje Wellness", Pershkrimi="Anti-slip 6mm", Cmimi=35, SasiaStok=20, Aktiv=true },
            new() { EmriProduktit="Peshqir Spa", Kategoria="Tekstile", Pershkrimi="100% pambuk premium", Cmimi=22, SasiaStok=40, Aktiv=true },
        };
        db.Produktet.AddRange(produktet);

        // 8. PROGRAMET
        var programet = new List<Models.Domain.Program>
        {
            new() { EmriProgramit="Humbje Peshe", Pershkrimi="Program 8 javësh", KohezgjatjaJave=8, Qellimi="Humbje peshe 5-8kg", Ushtrimet="Kardio 3x/javë", Dieta="Kalori të kufizuara" },
            new() { EmriProgramit="Relaksim & Stres", Pershkrimi="Program 4 javësh", KohezgjatjaJave=4, Qellimi="Reduktim stresi", Ushtrimet="Yoga 3x/javë", Dieta="Ushqim i ekuilibruar" },
            new() { EmriProgramit="Forcim Muskujsh", Pershkrimi="Program 12 javësh", KohezgjatjaJave=12, Qellimi="Rritje mase muskulore", Ushtrimet="Peshë 4x/javë", Dieta="Proteina të larta" },
        };
        db.Programet.AddRange(programet);

        await db.SaveChangesAsync();

        var rnd = new Random(42);
        var statuset = new[] { "Planifikuar", "Konfirmuar", "Perfunduar", "Anuluar" };

        // TERMINET
        var terminet = new List<Termin>();
        for (int i = 0; i < 25; i++)
        {
            var klient = klientet[rnd.Next(klientet.Count)];
            var sherbim = sherbimet[rnd.Next(sherbimet.Count)];
            var terapist = terapistet[rnd.Next(terapistet.Count)];
            var days = rnd.Next(-30, 30);
            var hour = rnd.Next(8, 17);
            terminet.Add(new Termin
            {
                KlientId = klient.KlientId,
                SherbimId = sherbim.SherbimId,
                TerapistId = terapist.TerapistId,
                DataTerminit = DateTime.UtcNow.AddDays(days),
                OraFillimit = new TimeSpan(hour, 0, 0),
                OraMbarimit = new TimeSpan(hour + 1, sherbim.KohezgjatjaMin % 60, 0),
                Statusi = days < -5 ? "Perfunduar" : days < 0 ? statuset[rnd.Next(4)] : "Planifikuar",
            });
        }
        db.Terminet.AddRange(terminet);

        // ANETARESIIMET
        var anetaresimet = new List<Anetaresim>();
        for (int i = 0; i < 10; i++)
        {
            var paketa = paketat[rnd.Next(paketat.Count)];
            var start = DateTime.UtcNow.AddMonths(-rnd.Next(0, 3));
            anetaresimet.Add(new Anetaresim
            {
                KlientId = klientet[i].KlientId,
                PaketId = paketa.PaketId,
                DataFillimit = start,
                DataMbarimit = start.AddMonths(paketa.KohezgjatjaMuaj),
                Statusi = "Aktiv",
                CmimiPaguar = paketa.Cmimi
            });
        }
        db.Anetaresimet.AddRange(anetaresimet);

        await db.SaveChangesAsync();

        var defaultKlient = klientet.FirstOrDefault();
        await EnsureUserWithRoleAsync(
            userManager,
            "client@wellness.com",
            "Client123!",
            defaultKlient?.Emri ?? "Client",
            defaultKlient?.Mbiemri ?? "Wellness",
            "Klient",
            defaultKlient?.KlientId.ToString());
    }

    private static async Task EnsureUserWithRoleAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        string? klientId = null)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                EmailConfirmed = true,
                KlientId = klientId
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded) return;
        }
        else if (role == "Klient" && string.IsNullOrWhiteSpace(user.KlientId) && !string.IsNullOrWhiteSpace(klientId))
        {
            user.KlientId = klientId;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
