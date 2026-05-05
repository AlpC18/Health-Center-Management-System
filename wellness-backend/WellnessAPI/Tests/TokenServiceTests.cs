using Xunit;
using Moq;
using FluentAssertions;
using WellnessAPI.Services;
using Microsoft.AspNetCore.Identity;
using WellnessAPI.Models.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;

namespace WellnessAPI.Tests;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _config;
    private readonly ApplicationDbContext _db;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _config = new Mock<IConfiguration>();

        _config.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyThatIsAtLeast32CharsLong!");
        _config.Setup(c => c["Jwt:Issuer"]).Returns("WellnessAPI");
        _config.Setup(c => c["Jwt:Audience"]).Returns("WellnessClient");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _userManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Admin" });

        _tokenService = new TokenService(_config.Object, _db, _userManager.Object);
    }

    [Fact]
    public async Task GenerateAccessToken_ShouldReturnValidTokenString()
    {
        var user = new ApplicationUser { Id = "test-id", Email = "test@wellness.com", FirstName = "Test", LastName = "User" };

        var token = await _tokenService.GenerateAccessTokenAsync(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldPersistHashedTokenAndReturnRawToken()
    {
        var user = new ApplicationUser { Id = "test-user-id" };

        var result = await _tokenService.GenerateRefreshTokenAsync(user, "127.0.0.1");

        result.RawToken.Should().NotBeNullOrEmpty();
        result.StoredToken.UserId.Should().Be("test-user-id");
        result.StoredToken.Token.Should().NotBe(result.RawToken);

        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == result.StoredToken.Token);
        stored.Should().NotBeNull();
    }
}
