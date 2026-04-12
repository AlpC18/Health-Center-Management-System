using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using WellnessAPI.Middleware;

namespace WellnessAPI.Tests.Middleware;

public class ErrorHandlingMiddlewareTests
{
    private static DefaultHttpContext BuildContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private static async Task<(int StatusCode, string Body)> InvokeWith(RequestDelegate pipeline)
    {
        var ctx = BuildContext();
        var middleware = new ErrorHandlingMiddleware(pipeline, NullLogger<ErrorHandlingMiddleware>.Instance);
        await middleware.InvokeAsync(ctx);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        return (ctx.Response.StatusCode, body);
    }

    [Fact]
    public async Task InvokeAsync_NoException_PassesThrough()
    {
        var called = false;
        var (status, _) = await InvokeWith(_ => { called = true; return Task.CompletedTask; });
        Assert.True(called);
        Assert.Equal(200, status);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        var (status, _) = await InvokeWith(_ => throw new UnauthorizedAccessException("test"));
        Assert.Equal((int)HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ReturnsJsonWithSuccessFalse()
    {
        var (_, body) = await InvokeWith(_ => throw new UnauthorizedAccessException("test"));
        var doc = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.False(doc.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ReturnsAlbanianErrorMessage()
    {
        var (_, body) = await InvokeWith(_ => throw new UnauthorizedAccessException("test"));
        var doc = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.Contains("autorizim", doc.GetProperty("error").GetString());
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        var (status, _) = await InvokeWith(_ => throw new Exception("boom"));
        Assert.Equal((int)HttpStatusCode.InternalServerError, status);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsJsonWithSuccessFalse()
    {
        var (_, body) = await InvokeWith(_ => throw new Exception("boom"));
        var doc = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.False(doc.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsAlbanianErrorMessage()
    {
        var (_, body) = await InvokeWith(_ => throw new Exception("boom"));
        var doc = JsonSerializer.Deserialize<JsonElement>(body);
        var error = doc.GetProperty("error").GetString();
        Assert.Contains("gabim", error);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_SetsContentTypeJson()
    {
        var ctx = BuildContext();
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new Exception("x"),
            NullLogger<ErrorHandlingMiddleware>.Instance);
        await middleware.InvokeAsync(ctx);
        Assert.Equal("application/json", ctx.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_SetsContentTypeJson()
    {
        var ctx = BuildContext();
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new UnauthorizedAccessException("x"),
            NullLogger<ErrorHandlingMiddleware>.Instance);
        await middleware.InvokeAsync(ctx);
        Assert.Equal("application/json", ctx.Response.ContentType);
    }
}
