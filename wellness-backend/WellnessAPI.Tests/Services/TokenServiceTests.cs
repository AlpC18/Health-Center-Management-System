using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using WellnessAPI.Data;
using WellnessAPI.Models.Identity;
using WellnessAPI.Services;

namespace WellnessAPI.Tests.Services;

public class TokenServiceTests
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly TokenService _sut;

    private static IConfiguration BuildConfig(string key = "TestKey_AtLeast32BytesLongForHmacSha256!")
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

    public TokenServiceTests()
    {
        _config = BuildConfig();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _userManager
            .Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Admin" });

        _sut = new TokenService(_config, _db, _userManager.Object);
    }

    private static ApplicationUser BuildUser() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Email = "test@test.com",
        UserName = "test@test.com",
        FirstName = "Test",
        LastName = "User"
    };

    // ── GenerateAccessToken ──────────────────────────────────────────────────

    [Fact]
    public async Task GenerateAccessToken_ReturnsNonEmptyString()
    {
        var token = await _sut.GenerateAccessTokenAsync(BuildUser());
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task GenerateAccessToken_IsValidJwt()
    {
        var user = BuildUser();
        var token = await _sut.GenerateAccessTokenAsync(user);

        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TestIssuer",
            ValidAudience = "TestAudience",
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        }, out _);

        Assert.NotNull(principal);
    }

    [Fact]
    public async Task GenerateAccessToken_ContainsEmailClaim()
    {
        var user = BuildUser();
        var token = await _sut.GenerateAccessTokenAsync(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

        Assert.Equal(user.Email, email);
    }

    [Fact]
    public async Task GenerateAccessToken_ContainsSubjectClaim()
    {
        var user = BuildUser();
        var token = await _sut.GenerateAccessTokenAsync(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var id = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        Assert.Equal(user.Id, id);
    }

    [Fact]
    public async Task GenerateAccessToken_ContainsNameClaims()
    {
        var user = BuildUser();
        var token = await _sut.GenerateAccessTokenAsync(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var firstName = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
        var lastName = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value;

        Assert.Equal(user.FirstName, firstName);
        Assert.Equal(user.LastName, lastName);
    }

    [Fact]
    public async Task GenerateAccessToken_ContainsJtiClaim()
    {
        var token = await _sut.GenerateAccessTokenAsync(BuildUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        Assert.NotEmpty(jti!);
    }

    [Fact]
    public async Task GenerateAccessToken_DifferentCallsProduceDifferentJti()
    {
        var user = BuildUser();
        var t1 = new JwtSecurityTokenHandler().ReadJwtToken(await _sut.GenerateAccessTokenAsync(user));
        var t2 = new JwtSecurityTokenHandler().ReadJwtToken(await _sut.GenerateAccessTokenAsync(user));

        var jti1 = t1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = t2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public async Task GenerateAccessToken_ExpiresInConfiguredMinutes()
    {
        var token = await _sut.GenerateAccessTokenAsync(BuildUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expected = DateTime.UtcNow.AddMinutes(60);
        Assert.True(Math.Abs((jwt.ValidTo - expected).TotalSeconds) < 5);
    }

    // ── GenerateRefreshToken ────────────────────────────────────────────────

    [Fact]
    public async Task GenerateRefreshToken_ReturnsNonEmptyToken()
    {
        var (storedToken, rawToken) = await _sut.GenerateRefreshTokenAsync(BuildUser("user-id"), "127.0.0.1");

        Assert.NotEmpty(rawToken);
        Assert.NotEmpty(storedToken.Token);
        Assert.NotEqual(rawToken, storedToken.Token);
    }

    [Fact]
    public async Task GenerateRefreshToken_AssignsUserId()
    {
        var (storedToken, _) = await _sut.GenerateRefreshTokenAsync(BuildUser("user-123"), "127.0.0.1");
        Assert.Equal("user-123", storedToken.UserId);
    }

    [Fact]
    public async Task GenerateRefreshToken_ExpiresInSevenDays()
    {
        var (storedToken, _) = await _sut.GenerateRefreshTokenAsync(BuildUser("uid"), "127.0.0.1");
        var expected = DateTime.UtcNow.AddDays(7);
        Assert.True(Math.Abs((storedToken.ExpiresAt - expected).TotalSeconds) < 5);
    }

    [Fact]
    public async Task GenerateRefreshToken_DifferentCallsProduceDifferentTokens()
    {
        var (_, rawToken1) = await _sut.GenerateRefreshTokenAsync(BuildUser("uid"), "127.0.0.1");
        var (_, rawToken2) = await _sut.GenerateRefreshTokenAsync(BuildUser("uid"), "127.0.0.1");
        Assert.NotEqual(rawToken1, rawToken2);
    }

    private static ApplicationUser BuildUser(string id) => new()
    {
        Id = id,
        Email = "test@test.com",
        UserName = "test@test.com",
        FirstName = "Test",
        LastName = "User"
    };
}
