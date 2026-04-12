using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WellnessAPI.Data;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
        // QuestPDF License (Community)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [HttpGet("klientet-pdf")]
    public async Task<IActionResult> ExportKlientet()
    {
        var klientet = await _db.Klientet.OrderBy(k => k.Emri).ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Wellness House Management").FontSize(20).SemiBold().FontColor(Colors.Green.Medium);
                        col.Item().Text("Lista e Klientëve të Regjistruar").FontSize(12).Italic();
                    });

                    row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(10);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("#");
                        header.Cell().Element(HeaderStyle).Text("Emri Mbiemri");
                        header.Cell().Element(HeaderStyle).Text("Email");
                        header.Cell().Element(HeaderStyle).Text("Telefoni");
                        header.Cell().Element(HeaderStyle).Text("Gjinia");

                        static IContainer HeaderStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        }
                    });

                    int i = 1;
                    foreach (var k in klientet)
                    {
                        table.Cell().Element(CellStyle).Text(i++.ToString());
                        table.Cell().Element(CellStyle).Text($"{k.Emri} {k.Mbiemri}");
                        table.Cell().Element(CellStyle).Text(k.Email);
                        table.Cell().Element(CellStyle).Text(k.Telefoni);
                        table.Cell().Element(CellStyle).Text(k.Gjinia);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Faqja ");
                    x.CurrentPageNumber();
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return File(stream.ToArray(), "application/pdf", $"Raporti_Klienteve_{DateTime.Now:yyyyMMdd}.pdf");
    }
}
