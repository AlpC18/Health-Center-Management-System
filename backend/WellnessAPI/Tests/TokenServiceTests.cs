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

        // Mock JWT Settings
        _config.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyThatIsAtLeast32CharsLong!");
        _config.Setup(c => c["Jwt:Issuer"]).Returns("WellnessAPI");
        _config.Setup(c => c["Jwt:Audience"]).Returns("WellnessClient");

        // Use In-Memory DB for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        // Mock UserManager
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _userManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Admin" });

        _tokenService = new TokenService(_config.Object, _db, _userManager.Object);
    }

    [Fact]
    public async Task GenerateAccessToken_ShouldReturnValidTokenString()
    {
        // Arrange
        var user = new ApplicationUser { Id = "test-id", Email = "test@wellness.com", FirstName = "Test", LastName = "User" };

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldPersistAndReturnToken()
    {
        // Arrange
        var user = new ApplicationUser { Id = "test-user-id" };

        // Act
        var result = await _tokenService.GenerateRefreshTokenAsync(user, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be("test-user-id");
        
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == result.Token);
        stored.Should().NotBeNull();
    }
}
