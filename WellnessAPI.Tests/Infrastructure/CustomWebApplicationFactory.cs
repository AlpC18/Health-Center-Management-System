using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WellnessAPI.Data;

namespace WellnessAPI.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "TestKey_AtLeast32BytesLongForHmacSha256!");
        builder.UseSetting("Jwt:Issuer", "TestIssuer");
        builder.UseSetting("Jwt:Audience", "TestAudience");
        builder.UseSetting("Jwt:ExpiryMinutes", "60");
        builder.UseSetting("SeedAdmin:Password", "Admin@12345!");
        builder.UseSetting("IpRateLimiting:EnableEndpointRateLimiting", "false");
        builder.UseSetting("IpRateLimiting:StackBlockedRequests", "false");

        builder.ConfigureServices(services =>
        {
            // Remove the real SQLite DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add InMemory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
        });

        builder.UseEnvironment("Testing");
    }
}
