using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using WellnessAPI.Data;
using WellnessAPI.Models.Identity;
using WellnessAPI.Services;
using WellnessAPI.Middleware;
using WellnessAPI.Hubs;
using WellnessAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

// 1. DB
builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    o.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o => {
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 8;
    o.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Jwt:Key must be provided via secure configuration (User Secrets or environment variables).");
}

builder.Services.AddAuthentication(o => {
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o => {
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// 5. Services & Validators
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<AppointmentReminderService>();
builder.Services.AddSignalR();
builder.Services.AddValidatorsFromAssemblyContaining<KlientValidators.Create>();
builder.Services.AddFluentValidationAutoValidation();

// 6. CORS
builder.Services.AddCors(o => o.AddPolicy("ReactApp", p =>
    p.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173")
     .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// 7. Swagger (JWT + XML Docs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Wellness House API",
        Version = "v1",
        Description = "RESTful API per sistemin e menaxhimit te Wellness House. Gjitha endpoints kerkojne JWT Bearer token.",
        Contact = new() { Name = "Wellness House Dev Team" }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Shkruani JWT tokenin tuaj."
    });
    c.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Migrate + Seed
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<ApplicationDbContext>();
    var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var rm = sp.GetRequiredService<RoleManager<IdentityRole>>();

    db.Database.Migrate();

    // Defensive schema patch: the AddAdressaToUser migration in this repo
    // was committed without the EF Designer partial, so EF skips it.
    // We ensure the column exists here so seeding and Identity queries work.
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE \"AspNetUsers\" ADD COLUMN \"Adresa\" TEXT NULL");
    }
    catch (Microsoft.Data.Sqlite.SqliteException) { /* column already exists */ }

    // Bootstrap tables for the additional CRUD entities.
    // Idempotent: CREATE TABLE IF NOT EXISTS won't touch existing tables.
    var ddl = new[]
    {
        @"CREATE TABLE IF NOT EXISTS ""Sallat"" (
            ""SallaId"" INTEGER NOT NULL CONSTRAINT ""PK_Sallat"" PRIMARY KEY AUTOINCREMENT,
            ""Emri"" TEXT NOT NULL,
            ""Kapaciteti"" INTEGER NOT NULL,
            ""Tipi"" TEXT NULL,
            ""Pershkrimi"" TEXT NULL,
            ""Aktive"" INTEGER NOT NULL DEFAULT 1
        );",
        @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Sallat_Emri"" ON ""Sallat"" (""Emri"");",

        @"CREATE TABLE IF NOT EXISTS ""Furnizuesit"" (
            ""FurnizuesId"" INTEGER NOT NULL CONSTRAINT ""PK_Furnizuesit"" PRIMARY KEY AUTOINCREMENT,
            ""Emri"" TEXT NOT NULL,
            ""KontaktPersona"" TEXT NULL,
            ""Email"" TEXT NULL,
            ""Telefoni"" TEXT NULL,
            ""Adresa"" TEXT NULL,
            ""Aktiv"" INTEGER NOT NULL DEFAULT 1,
            ""DataRegjistrimit"" TEXT NOT NULL
        );",
        @"CREATE INDEX IF NOT EXISTS ""IX_Furnizuesit_Emri"" ON ""Furnizuesit"" (""Emri"");",

        @"CREATE TABLE IF NOT EXISTS ""Lajmerimet"" (
            ""LajmerimId"" INTEGER NOT NULL CONSTRAINT ""PK_Lajmerimet"" PRIMARY KEY AUTOINCREMENT,
            ""Titulli"" TEXT NOT NULL,
            ""Permbajtja"" TEXT NOT NULL,
            ""Audienca"" TEXT NOT NULL DEFAULT 'All',
            ""Prioriteti"" TEXT NOT NULL DEFAULT 'Mesem',
            ""DataKrijimit"" TEXT NOT NULL,
            ""DataSkadimit"" TEXT NULL,
            ""Aktiv"" INTEGER NOT NULL DEFAULT 1
        );",
        @"CREATE INDEX IF NOT EXISTS ""IX_Lajmerimet_DataKrijimit"" ON ""Lajmerimet"" (""DataKrijimit"");",

        @"CREATE TABLE IF NOT EXISTS ""Zbritjet"" (
            ""ZbritjeId"" INTEGER NOT NULL CONSTRAINT ""PK_Zbritjet"" PRIMARY KEY AUTOINCREMENT,
            ""Kodi"" TEXT NOT NULL,
            ""PerqindjaZbritjes"" decimal(5,2) NOT NULL,
            ""DataFillimit"" TEXT NOT NULL,
            ""DataMbarimit"" TEXT NOT NULL,
            ""LimitiPerdorimit"" INTEGER NOT NULL DEFAULT 100,
            ""HereshShfrytezuar"" INTEGER NOT NULL DEFAULT 0,
            ""Aktive"" INTEGER NOT NULL DEFAULT 1
        );",
        @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Zbritjet_Kodi"" ON ""Zbritjet"" (""Kodi"");",

        @"CREATE TABLE IF NOT EXISTS ""Pushimet"" (
            ""PushimId"" INTEGER NOT NULL CONSTRAINT ""PK_Pushimet"" PRIMARY KEY AUTOINCREMENT,
            ""TerapistId"" INTEGER NOT NULL,
            ""DataFillimit"" TEXT NOT NULL,
            ""DataMbarimit"" TEXT NOT NULL,
            ""Arsyeja"" TEXT NULL,
            ""Statusi"" TEXT NOT NULL DEFAULT 'Kerkuar',
            ""DataKerkimit"" TEXT NOT NULL
        );",
        @"CREATE INDEX IF NOT EXISTS ""IX_Pushimet_TerapistId"" ON ""Pushimet"" (""TerapistId"");",
        @"CREATE INDEX IF NOT EXISTS ""IX_Pushimet_Statusi"" ON ""Pushimet"" (""Statusi"");",
    };
    foreach (var sql in ddl) db.Database.ExecuteSqlRaw(sql);

    SeedData.SeedAsync(db, um, rm).Wait();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseIpRateLimiting();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
