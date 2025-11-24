using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

/// <summary>
/// Servicio para generar reportes PDF del Motor de Inferencia
/// </summary>
public class ReportesMotorService
{
    private readonly ApplicationDbContext _context;

    public ReportesMotorService(ApplicationDbContext context)
    {
        _context = context;
        
        // Configuración de licencia QuestPDF (Community License)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera un reporte PDF completo del motor de inferencia
    /// </summary>
    public async Task<byte[]> GenerarReportePdfAsync(DateOnly? fechaCorte = null, int? programaId = null)
    {
        fechaCorte ??= DateOnly.FromDateTime(DateTime.Now);

        // Obtener datos
        var programa = programaId.HasValue 
            ? await _context.Programas.FirstOrDefaultAsync(p => p.ProgramaId == programaId && !p.IsDeleted)
            : null;

        var alertas = await ObtenerAlertasAsync(fechaCorte.Value, programaId);
        var estadisticas = await ObtenerEstadisticasAsync(fechaCorte.Value, programaId);
        var ultimaEjecucion = await _context.EjecucionesMotor
            .OrderByDescending(e => e.EjecucionId)
            .FirstOrDefaultAsync();

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
                page.Header().Element(c => ComposeHeader(c, programa, fechaCorte.Value));

                // Content
                page.Content().Element(c => ComposeContent(c, alertas, estadisticas, ultimaEjecucion));

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

    private void ComposeHeader(IContainer container, Domain.Programas.Programa? programa, DateOnly fechaCorte)
    {
        container.Column(column =>
        {
            // Título principal
            column.Item().Background("#4D3935").Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Motor de Inferencia - Reporte de Alertas")
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.White);

                    var subtitulo = $"Fecha de Corte: {fechaCorte:dd/MM/yyyy}";
                    if (programa != null)
                    {
                        subtitulo += $" | Programa: {programa.Nombre}";
                    }

                    col.Item().PaddingTop(5).Text(subtitulo)
                        .FontSize(11)
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

    private void ComposeContent(IContainer container, List<AlertaReporte> alertas, 
        EstadisticasMotor estadisticas, Domain.Motor.EjecucionMotor? ultimaEjecucion)
    {
        container.Column(column =>
        {
            // KPIs
            column.Item().Element(c => ComposeKPIs(c, estadisticas, ultimaEjecucion));

            column.Item().PaddingTop(20);

            // Resumen por Severidad
            column.Item().Element(c => ComposeResumenSeveridad(c, alertas));

            column.Item().PaddingTop(20);

            // Resumen por Estado
            column.Item().Element(c => ComposeResumenEstado(c, alertas));

            column.Item().PaddingTop(20);

            // Tabla de Alertas
            column.Item().Element(c => ComposeTablaAlertas(c, alertas));
        });
    }

    private void ComposeKPIs(IContainer container, EstadisticasMotor stats, Domain.Motor.EjecucionMotor? ejecucion)
    {
        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text("Indicadores Clave (KPIs)")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Element(c => ComposeKPICard(c, "Total Alertas", stats.TotalAlertas.ToString(), "#9FD996"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeKPICard(c, "Alertas Abiertas", stats.AlertasAbiertas.ToString(), "#F3C95A"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeKPICard(c, "Alertas Resueltas", stats.AlertasResueltas.ToString(), "#9FD996"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeKPICard(c, "Alertas Críticas", stats.AlertasCriticas.ToString(), "#F7C484"));
            });

            if (ejecucion != null)
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKPICard(c, "Última Ejecución", 
                        ejecucion.InicioUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"), "#FEFEFD"));
                    row.Spacing(10);
                    row.RelativeItem().Element(c => ComposeKPICard(c, "Reglas Ejecutadas", 
                        stats.ReglasActivas.ToString(), "#FEFEFD"));
                    row.Spacing(10);
                    row.RelativeItem().Element(c => ComposeKPICard(c, "Programas Analizados", 
                        stats.ProgramasAnalizados.ToString(), "#FEFEFD"));
                });
            }
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

    private void ComposeResumenSeveridad(IContainer container, List<AlertaReporte> alertas)
    {
        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text("Distribución por Severidad")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Row(row =>
            {
                var criticas = alertas.Count(a => a.Severidad == "Crítica");
                var altas = alertas.Count(a => a.Severidad == "Alta");
                var info = alertas.Count(a => a.Severidad == "Info");

                row.RelativeItem().Element(c => ComposeSeveridadBar(c, "Crítica", criticas, alertas.Count, "#F7C484"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeSeveridadBar(c, "Alta", altas, alertas.Count, "#F3C95A"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeSeveridadBar(c, "Info", info, alertas.Count, "#9FD996"));
            });
        });
    }

    private void ComposeSeveridadBar(IContainer container, string severidad, int cantidad, int total, string color)
    {
        var porcentaje = total > 0 ? (cantidad * 100.0 / total) : 0;

        container.Border(1).BorderColor("#e0e0e0").Padding(10).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(severidad).FontSize(11).SemiBold();
                row.ConstantItem(50).AlignRight().Text($"{cantidad}").FontSize(11).Bold();
            });

            col.Item().PaddingTop(5).Height(20).Background("#e0e0e0").Row(inner =>
            {
                var width = (float)porcentaje;
                if (width > 0)
                {
                    inner.ConstantItem(width).Height(20).Background(color);
                }
            });

            col.Item().PaddingTop(3).AlignRight().Text($"{porcentaje:F1}%").FontSize(9).FontColor("#6D534F");
        });
    }

    private void ComposeResumenEstado(IContainer container, List<AlertaReporte> alertas)
    {
        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text("Distribución por Estado")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Row(row =>
            {
                var abiertas = alertas.Count(a => a.Estado == "Abierta");
                var resueltas = alertas.Count(a => a.Estado == "Resuelta");
                var descartadas = alertas.Count(a => a.Estado == "Descartada");

                row.RelativeItem().Element(c => ComposeEstadoCard(c, "Abiertas", abiertas, "#F3C95A"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeEstadoCard(c, "Resueltas", resueltas, "#9FD996"));
                row.Spacing(10);
                row.RelativeItem().Element(c => ComposeEstadoCard(c, "Descartadas", descartadas, "#4D3935"));
            });
        });
    }

    private void ComposeEstadoCard(IContainer container, string estado, int cantidad, string color)
    {
        container.Border(1).BorderColor("#e0e0e0").Padding(15).Column(col =>
        {
            col.Item().AlignCenter().Text(estado).FontSize(11).SemiBold().FontColor("#6D534F");
            col.Item().PaddingTop(8).AlignCenter().Text(cantidad.ToString()).FontSize(24).Bold().FontColor(color);
        });
    }

    private void ComposeTablaAlertas(IContainer container, List<AlertaReporte> alertas)
    {
        container.Column(column =>
        {
            column.Item().Background("#F7C484").Padding(10).Text($"Detalle de Alertas ({alertas.Count})")
                .FontSize(14).Bold().FontColor("#4D3935");

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // ID
                    columns.ConstantColumn(60);  // Severidad
                    columns.RelativeColumn(3);   // Mensaje
                    columns.ConstantColumn(80);  // Programa
                    columns.ConstantColumn(60);  // Fecha
                    columns.ConstantColumn(60);  // Estado
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background("#4D3935").Padding(5).Text("ID").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Severidad").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Mensaje").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Programa").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Generada").FontSize(9).Bold().FontColor(Colors.White);
                    header.Cell().Background("#4D3935").Padding(5).Text("Estado").FontSize(9).Bold().FontColor(Colors.White);
                });

                // Rows
                foreach (var alerta in alertas.Take(100)) // Máximo 100 alertas
                {
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.AlertaId.ToString()).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.Severidad).FontSize(8).SemiBold();
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.Mensaje).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.ProgramaNombre ?? "-").FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.GeneradaEn.ToString("dd/MM/yyyy")).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor("#e0e0e0").Padding(5).Text(alerta.Estado).FontSize(8);
                }
            });

            if (alertas.Count > 100)
            {
                column.Item().PaddingTop(10).Text($"Mostrando las primeras 100 de {alertas.Count} alertas totales")
                    .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private async Task<List<AlertaReporte>> ObtenerAlertasAsync(DateOnly fechaCorte, int? programaId)
    {
        var query = _context.Alertas
            .Include(a => a.Programa)
            .Include(a => a.Regla)
            .Where(a => !a.IsDeleted && a.GeneradaEn <= fechaCorte.ToDateTime(TimeOnly.MaxValue));

        if (programaId.HasValue)
            query = query.Where(a => a.ProgramaId == programaId.Value);

        var alertas = await query
            .OrderByDescending(a => a.GeneradaEn)
            .Select(a => new AlertaReporte
            {
                AlertaId = a.AlertaId,
                Mensaje = a.Mensaje,
                Severidad = a.Severidad.ToString(),
                Estado = a.Estado.ToString(),
                GeneradaEn = a.GeneradaEn,
                ProgramaNombre = a.Programa != null ? a.Programa.Nombre : null,
                ReglaNombre = a.Regla != null ? a.Regla.Nombre : null
            })
            .ToListAsync();

        return alertas;
    }

    private async Task<EstadisticasMotor> ObtenerEstadisticasAsync(DateOnly fechaCorte, int? programaId)
    {
        var query = _context.Alertas
            .Where(a => !a.IsDeleted && a.GeneradaEn <= fechaCorte.ToDateTime(TimeOnly.MaxValue));

        if (programaId.HasValue)
            query = query.Where(a => a.ProgramaId == programaId.Value);

        var totalAlertas = await query.CountAsync();
        var alertasAbiertas = await query.CountAsync(a => a.Estado == EstadoAlerta.Abierta);
        var alertasResueltas = await query.CountAsync(a => a.Estado == EstadoAlerta.Resuelta);
        var alertasCriticas = await query.CountAsync(a => a.Severidad == Severidad.Critica);

        var reglasActivas = await _context.Reglas.CountAsync(r => r.Activa && !r.IsDeleted);

        var programasQuery = _context.Programas.Where(p => !p.IsDeleted);
        if (programaId.HasValue)
            programasQuery = programasQuery.Where(p => p.ProgramaId == programaId.Value);
        var programasAnalizados = await programasQuery.CountAsync();

        return new EstadisticasMotor
        {
            TotalAlertas = totalAlertas,
            AlertasAbiertas = alertasAbiertas,
            AlertasResueltas = alertasResueltas,
            AlertasCriticas = alertasCriticas,
            ReglasActivas = reglasActivas,
            ProgramasAnalizados = programasAnalizados
        };
    }

    private class AlertaReporte
    {
        public int AlertaId { get; set; }
        public string Mensaje { get; set; } = "";
        public string Severidad { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime GeneradaEn { get; set; }
        public string? ProgramaNombre { get; set; }
        public string? ReglaNombre { get; set; }
    }

    private class EstadisticasMotor
    {
        public int TotalAlertas { get; set; }
        public int AlertasAbiertas { get; set; }
        public int AlertasResueltas { get; set; }
        public int AlertasCriticas { get; set; }
        public int ReglasActivas { get; set; }
        public int ProgramasAnalizados { get; set; }
    }
}
