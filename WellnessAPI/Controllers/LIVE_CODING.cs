/*
// This file contains snippets to be used during live coding.
// It is not a real controller, just ready-made code for copy-pasting.

// SNIPPET 1 — Add a new simple endpoint (to any controller)
[HttpGet("count")]
public async Task<IActionResult> GetCount()
{
    var count = await _db.Klientet.CountAsync();
    return Ok(new { count, message = "Client count" });
}

// SNIPPET 2 — Add search endpoint
[HttpGet("search")]
public async Task<IActionResult> Search([FromQuery] string q)
{
    var results = await _db.Klientet
        .Where(k => k.Emri.Contains(q) || k.Email.Contains(q))
        .Select(k => new { k.KlientId, k.Emri, k.Mbiemri, k.Email })
        .Take(10)
        .ToListAsync();
    return Ok(results);
}

// SNIPPET 3 — Add a new field (to DTO)
// Example of adding a new field to an existing DTO in AllDtos.cs
// public record KlientCreateDto(..., string? Instagram = null);

// SNIPPET 4 — Add a new input in Frontend (in Forms.jsx)
// <Input label="Instagram" value={f.instagram} onChange={set('instagram')} />

// SNIPPET 5 — Simple statistics endpoint
[HttpGet("stats/by-gender")]
public async Task<IActionResult> StatsByGender()
{
    var stats = await _db.Klientet
        .GroupBy(k => k.Gjinia ?? "Pa specifikuar")
        .Select(g => new { gjinia = g.Key, count = g.Count() })
        .ToListAsync();
    return Ok(stats);
}

// SNIPPET 6 — Get user information from Token
private string? GetUserId() =>
    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
    ?? User.FindFirst("sub")?.Value;

private string? GetUserRole() =>
    User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
    ?? User.FindFirst("role")?.Value;
*/
