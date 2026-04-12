using System.Net;
using System.Text.Json;

namespace WellnessAPI.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized");
            await Write(context, HttpStatusCode.Unauthorized, "Qasja nuk lejohet.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await Write(context, HttpStatusCode.InternalServerError,
                "Ndodhi një gabim i papritur.");
        }
    }

    private static async Task Write(HttpContext ctx, HttpStatusCode code, string msg)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)code;
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            success = false,
            message = msg,
            statusCode = (int)code,
            timestamp = DateTime.UtcNow
        }));
    }
}
