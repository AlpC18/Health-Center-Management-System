using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using ClosedXML.Excel;

namespace WellnessAPI.Controllers;

/// <summary>
/// Kontrolluesi për eksportimin e të dhënave në formate të ndryshme (Excel).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ExportController(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Eksporton listën e klientëve në formatin Excel (.xlsx).
    /// </summary>
    /// <returns>Një skedar Excel me të dhënat e klientëve.</returns>
    [HttpGet("klientet/excel")]
    public async Task<IActionResult> ExportKlientetExcel()
    {
        var data = await _db.Klientet.AsNoTracking().ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Klientët");
        
        // Header
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Emri";
        worksheet.Cell(1, 3).Value = "Mbiemri";
        worksheet.Cell(1, 4).Value = "Email";
        worksheet.Cell(1, 5).Value = "Telefoni";
        worksheet.Cell(1, 6).Value = "Data Regjistrimit";

        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        for (int i = 0; i < data.Count; i++)
        {
            var k = data[i];
            worksheet.Cell(i + 2, 1).Value = k.KlientId;
            worksheet.Cell(i + 2, 2).Value = k.Emri;
            worksheet.Cell(i + 2, 3).Value = k.Mbiemri;
            worksheet.Cell(i + 2, 4).Value = k.Email;
            worksheet.Cell(i + 2, 5).Value = k.Telefoni;
            worksheet.Cell(i + 2, 6).Value = k.DataRegjistrimit.ToString("yyyy-MM-dd");
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Wellness_Klientet.xlsx");
    }

    /// <summary>
    /// Eksporton listën e termineve në formatin Excel (.xlsx).
    /// </summary>
    /// <returns>Një skedar Excel me të dhënat e termineve.</returns>
    [HttpGet("terminet/excel")]
    public async Task<IActionResult> ExportTerminetExcel()
    {
        var data = await _db.Terminet
            .Include(t => t.Klienti)
            .Include(t => t.Sherbimi)
            .Include(t => t.Terapisti)
            .AsNoTracking().ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Terminet");
        
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Klienti";
        worksheet.Cell(1, 3).Value = "Shërbimi";
        worksheet.Cell(1, 4).Value = "Terapisti";
        worksheet.Cell(1, 5).Value = "Data";
        worksheet.Cell(1, 6).Value = "Ora";
        worksheet.Cell(1, 7).Value = "Statusi";

        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.AirForceBlue;
        headerRange.Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < data.Count; i++)
        {
            var t = data[i];
            worksheet.Cell(i + 2, 1).Value = t.TerminId;
            worksheet.Cell(i + 2, 2).Value = $"{t.Klienti?.Emri} {t.Klienti?.Mbiemri}";
            worksheet.Cell(i + 2, 3).Value = t.Sherbimi?.EmriSherbimit;
            worksheet.Cell(i + 2, 4).Value = $"{t.Terapisti?.Emri} {t.Terapisti?.Mbiemri}";
            worksheet.Cell(i + 2, 5).Value = t.DataTerminit.ToString("yyyy-MM-dd");
            worksheet.Cell(i + 2, 6).Value = $"{t.OraFillimit} - {t.OraMbarimit}";
            worksheet.Cell(i + 2, 7).Value = t.Statusi;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Wellness_Terminet.xlsx");
    }
}
