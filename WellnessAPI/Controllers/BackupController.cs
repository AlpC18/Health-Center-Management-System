using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BackupController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public BackupController(IWebHostEnvironment env) => _env = env;

    [HttpGet("database")]
    public IActionResult ExportDatabase()
    {
        // SQLite database file path (from root)
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "wellness.db");

        if (!System.IO.File.Exists(dbPath))
            return NotFound(new { message = "Veritabanı dosyası bulunamadı." });

        // Generate a filename with current date
        var fileName = $"WellnessBackup_{DateTime.Now:yyyyMMdd_HHmm}.db";

        // Read bytes to allow file access even if DB is open (In some cases might need Copy)
        var tempFile = Path.Combine(Path.GetTempPath(), fileName);
        System.IO.File.Copy(dbPath, tempFile, true);
        
        var bytes = System.IO.File.ReadAllBytes(tempFile);
        System.IO.File.Delete(tempFile);

        return File(bytes, "application/x-sqlite3", fileName);
    }
}
