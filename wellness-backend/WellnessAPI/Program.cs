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
var dbProvider = (builder.Configuration["DatabaseProvider"] ?? "MySql").Trim();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        var conn = builder.Configuration.GetConnectionString("MySqlConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:MySqlConnection");
        options.UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36)));
    }
    else if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var conn = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        options.UseSqlite(conn);
    }
    else
    {
        throw new InvalidOperationException($"Unsupported DatabaseProvider '{dbProvider}'. Use 'MySql' or 'Sqlite'.");
    }
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
var jwtKey = builder.Configuration["Jwt:Key"]!;
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
        Description = "RESTful API për sistemin e menaxhimit të Wellness House. Gjitha endpoints kërkojnë JWT Bearer token.",
        Contact = new() { Name = "Wellness House Dev Team" }
    });

    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    // JWT Bearer button in Swagger UI — Swashbuckle 10 / OpenAPI v2 pattern
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
    SeedData.SeedAsync(db, um, rm).Wait();
}

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
