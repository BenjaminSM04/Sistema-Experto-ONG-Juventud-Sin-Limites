using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

/// <summary>
/// Servicio para generar reportes PDF de programas
/// </summary>
public class ReportesProgramaService
{
    private readonly ApplicationDbContext _context;

    public ReportesProgramaService(ApplicationDbContext context)
    {
        _context = context;
        // Configuración de licencia QuestPDF (Community License)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera un reporte PDF detallado del programa para un año específico
    /// </summary>
    public async Task<byte[]> GenerarReportePdfAsync(int programaId, int anio)
    {
        var programa = await _context.Programas
            .FirstOrDefaultAsync(p => p.ProgramaId == programaId && !p.IsDeleted);

        if (programa == null)
            throw new InvalidOperationException("Programa no encontrado");

        var actividades = await _context.Actividades
            .Where(a => a.ProgramaId == programaId &&
                        !a.IsDeleted &&
                        a.FechaInicio.Year == anio)
            .OrderBy(a => a.FechaInicio)
            .ToListAsync();

        var metricas = await _context.MetricasProgramaMes
            .Where(m => m.ProgramaId == programaId &&
                        !m.IsDeleted &&
                        m.AnioMes.StartsWith(anio.ToString()))
            .OrderBy(m => m.AnioMes)
            .ToListAsync();

        var alertas = await _context.Alertas
            .Include(a => a.Regla)
            .Where(a => a.ProgramaId == programaId &&
                        !a.IsDeleted &&
                        a.GeneradaEn.Year == anio)
            .OrderByDescending(a => a.GeneradaEn)
            .ToListAsync();

        // Generar PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // Header
                page.Header().Element(c => ComposeHeader(c, programa, anio));

                // Content
                page.Content().Element(c => ComposeContent(c, actividades, metricas, alertas, anio));

                // Footer
                page.Footer().AlignCenter().Column(column =>
                {
                    column.Item().Text($"Página {{number}} de {{total}} | Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Domain.Programas.Programa programa, int anio)
    {
        container.Column(column =>
        {
            // Título principal
            column.Item().Background("#4D3935").Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(programa.Nombre)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.White);

                    col.Item().PaddingTop(5).Text($"Clave: {programa.Clave} | Año: {anio}")
                        .FontSize(11)
                        .FontColor(Colors.White);
                    
                    col.Item().Text(programa.Descripcion ?? "")
                        .FontSize(10)
                        .FontColor(Colors.White);
                });

                row.ConstantItem(100).AlignRight().Text("ONG\nJuventud\nSin Límites")
                    .FontSize(10)
                    .Bold()
                    .FontColor(Colors.White);
            });

            column.Item().PaddingTop(10);
        });
    }

    private void ComposeContent(IContainer container, 
        List<Domain.Operacion.Actividad> actividades,
        List<Domain.BI.MetricasProgramaMes> metricas,
        List<Domain.Motor.Alerta> alertas,
        int anio)
    {
        container.Column(column =>
        {
            // KPIs
            column.Item().Element(c => ComposeKPIs(c, metricas, anio));

            column.Item().PaddingTop(20);

            // Actividades
            column.Item().Element(c => ComposeActividades(c, actividades));

            column.Item().PaddingTop(20);

            // Métricas Mensuales
            column.Item().Element(c => ComposeMetricas(c, metricas));

            column.Item().PaddingTop(20);

            // Alertas
            column.Item().Element(c => ComposeAlertas(c, alertas));
        });
    }

    private void ComposeKPIs(IContainer container, List<Domain.BI.MetricasProgramaMes> metricas, int anio)
    {
        var totalPlanificadas = metricas.Sum(m => m.ActividadesPlanificadas);
        var totalEjecutadas = metricas.Sum(m => m.ActividadesEjecutadas);
        var cumplimientoPromedio = metricas.Any() ? metricas.Average(m => (double)m.PorcCumplimiento) : 0;
        var asistenciaPromedio = metricas.Any() ? metricas.Average(m => (double)m.PorcAsistenciaProm) : 0;

        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text($"KPIs Anuales {anio}")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Element(c => ComposeKPICard(c, "Actividades Planificadas", totalPlanificadas.ToString(), "#FEFEFD"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeKPICard(c, "Actividades Ejecutadas", totalEjecutadas.ToString(), "#FEFEFD"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeKPICard(c, "% Cumplimiento Promedio", $"{cumplimientoPromedio:F1}%", "#FEFEFD"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeKPICard(c, "% Asistencia Promedio", $"{asistenciaPromedio:F1}%", "#FEFEFD"));
            });
        });
    }

    private void ComposeKPICard(IContainer container, string label, string value, string color)
    {
        container.Border(1).BorderColor("#e0e0e0").Background(color).Padding(10).Column(col =>
        {
            col.Item().Text(label).FontSize(9).FontColor("#6D534F").SemiBold();
            col.Item().PaddingTop(5).Text(value).FontSize(20).Bold().FontColor("#4D3935");
        });
    }

    private void ComposeActividades(IContainer container, List<Domain.Operacion.Actividad> actividades)
    {
        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text($"Actividades Realizadas ({actividades.Count})")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);   // Título
                    columns.ConstantColumn(80);  // Fecha
                    columns.ConstantColumn(80);  // Tipo
                    columns.ConstantColumn(80);  // Estado
                    columns.RelativeColumn(2);   // Lugar
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background("#4D3935").Padding(5).Text("Título").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Fecha").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Tipo").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Estado").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Lugar").FontSize(9).Bold().FontColor(Colors.White);
                });

                // Rows
                foreach (var actividad in actividades)
                {
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(actividad.Titulo).FontSize(8).Bold();
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(actividad.FechaInicio.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(actividad.Tipo.ToString()).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(actividad.Estado.ToString()).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(actividad.Lugar ?? "N/A").FontSize(8);
                }
            });
        });
    }

    private void ComposeMetricas(IContainer container, List<Domain.BI.MetricasProgramaMes> metricas)
    {
        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text("Métricas Mensuales")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);  // Mes
                    columns.RelativeColumn();    // Planificadas
                    columns.RelativeColumn();    // Ejecutadas
                    columns.RelativeColumn();    // % Cumplimiento
                    columns.RelativeColumn();    // % Asistencia
                    columns.RelativeColumn();    // Retraso
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background("#4D3935").Padding(5).Text("Mes").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Planificadas").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Ejecutadas").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("% Cumplimiento").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("% Asistencia").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Retraso (días)").FontSize(9).Bold().FontColor(Colors.White);
                });

                // Rows
                foreach (var metrica in metricas)
                {
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(metrica.AnioMes).FontSize(8).Bold();
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(metrica.ActividadesPlanificadas.ToString()).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(metrica.ActividadesEjecutadas.ToString()).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text($"{metrica.PorcCumplimiento:F1}%").FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text($"{metrica.PorcAsistenciaProm:F1}%").FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text($"{metrica.RetrasoPromedioDias:F1}").FontSize(8);
                }
            });
        });
    }

    private void ComposeAlertas(IContainer container, List<Domain.Motor.Alerta> alertas)
    {
        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text($"Alertas Generadas ({alertas.Count})")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);  // Fecha
                    columns.ConstantColumn(60);  // Severidad
                    columns.ConstantColumn(100); // Regla
                    columns.RelativeColumn(3);   // Mensaje
                    columns.ConstantColumn(60);  // Estado
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background("#4D3935").Padding(5).Text("Fecha").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Severidad").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Regla").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Mensaje").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Estado").FontSize(9).Bold().FontColor(Colors.White);
                });

                // Rows
                foreach (var alerta in alertas.Take(50))
                {
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.GeneradaEn.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.Severidad.ToString()).FontSize(8).SemiBold();
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.Regla?.Nombre ?? "N/A").FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.Mensaje).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.Estado.ToString()).FontSize(8);
                }
            });
        });
    }
}
