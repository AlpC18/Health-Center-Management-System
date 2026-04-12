using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WellnessAPI.Models.Identity;
using WellnessAPI.Services;

namespace WellnessAPI.Tests.Services;

public class TokenServiceTests
{
    private readonly IConfiguration _config;
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
        _sut = new TokenService(_config);
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
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateAccessToken(BuildUser());
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateAccessToken_IsValidJwt()
    {
        var user = BuildUser();
        var token = _sut.GenerateAccessToken(user);

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
    public void GenerateAccessToken_ContainsEmailClaim()
    {
        var user = BuildUser();
        var token = _sut.GenerateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        Assert.Equal(user.Email, email);
    }

    [Fact]
    public void GenerateAccessToken_ContainsNameIdentifierClaim()
    {
        var user = BuildUser();
        var token = _sut.GenerateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var id = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        Assert.Equal(user.Id, id);
    }

    [Fact]
    public void GenerateAccessToken_ContainsFullNameClaim()
    {
        var user = BuildUser();
        var token = _sut.GenerateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var name = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        Assert.Equal($"{user.FirstName} {user.LastName}", name);
    }

    [Fact]
    public void GenerateAccessToken_ContainsJtiClaim()
    {
        var token = _sut.GenerateAccessToken(BuildUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        Assert.NotEmpty(jti!);
    }

    [Fact]
    public void GenerateAccessToken_DifferentCallsProduceDifferentJti()
    {
        var user = BuildUser();
        var t1 = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateAccessToken(user));
        var t2 = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateAccessToken(user));

        var jti1 = t1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = t2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public void GenerateAccessToken_ExpiresInConfiguredMinutes()
    {
        var token = _sut.GenerateAccessToken(BuildUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expected = DateTime.UtcNow.AddMinutes(60);
        Assert.True(Math.Abs((jwt.ValidTo - expected).TotalSeconds) < 5);
    }

    // ── GenerateRefreshToken ────────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyToken()
    {
        var rt = _sut.GenerateRefreshToken("user-id");
        Assert.NotEmpty(rt.Token);
    }

    [Fact]
    public void GenerateRefreshToken_AssignsUserId()
    {
        var rt = _sut.GenerateRefreshToken("user-123");
        Assert.Equal("user-123", rt.UserId);
    }

    [Fact]
    public void GenerateRefreshToken_ExpiresInSevenDays()
    {
        var rt = _sut.GenerateRefreshToken("uid");
        var expected = DateTime.UtcNow.AddDays(7);
        Assert.True(Math.Abs((rt.Expires - expected).TotalSeconds) < 5);
    }

    [Fact]
    public void GenerateRefreshToken_DifferentCallsProduceDifferentTokens()
    {
        var t1 = _sut.GenerateRefreshToken("uid");
        var t2 = _sut.GenerateRefreshToken("uid");
        Assert.NotEqual(t1.Token, t2.Token);
    }
}
