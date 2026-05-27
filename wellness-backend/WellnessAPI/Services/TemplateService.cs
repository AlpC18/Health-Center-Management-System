using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Services;

public class TemplateService
{
    private readonly ApplicationDbContext _db;

    public TemplateService(ApplicationDbContext db) => _db = db;

    public async Task<(string? Subject, string Body)> RenderAsync(
        string key,
        TemplateChannel channel,
        string? fallbackSubject,
        string fallbackBody,
        IReadOnlyDictionary<string, string?> tokens,
        CancellationToken cancellationToken = default)
    {
        var template = await _db.Templates.AsNoTracking()
            .Where(t => t.Key == key && t.Channel == channel && t.Active)
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var subject = Render(template?.Subject ?? fallbackSubject, tokens);
        var body = Render(template?.Body ?? fallbackBody, tokens);
        return (subject, body);
    }

    private static string Render(string? input, IReadOnlyDictionary<string, string?> tokens)
    {
        var output = input ?? string.Empty;
        foreach (var (key, value) in tokens)
        {
            output = output
                .Replace("{{" + key + "}}", value ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("{" + key + "}", value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        return output;
    }
}
