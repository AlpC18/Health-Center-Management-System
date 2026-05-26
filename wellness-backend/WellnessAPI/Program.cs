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
    if (builder.Environment.IsEnvironment("Testing"))
    {
        o.UseInMemoryDatabase("WellnessApiTests");
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        // AutoDetect adapts to whatever the team runs: MariaDB (XAMPP) or MySQL (Laragon/Docker).
        o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
});

// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o => {
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 8;
    o.User.RequireUniqueEmail = true;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    o.Lockout.AllowedForNewUsers = true;
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

    if (db.Database.IsRelational())
    {
        db.Database.Migrate();

    }

    SeedData.SeedAsync(db, um, rm).Wait();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseIpRateLimiting();
}
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();

public partial class Program { }
