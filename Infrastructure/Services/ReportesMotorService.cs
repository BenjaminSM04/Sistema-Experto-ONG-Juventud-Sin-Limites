using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

/// <summary>
/// Servicio para generar reportes PDF del Motor de Inferencia con dashboard completo
/// </summary>
public class ReportesMotorService
{
    private readonly ApplicationDbContext _context;

    // Colores del tema Mythra
    private const string ColorPrimario = "#4D3935";
    private const string ColorSecundario = "#6D534F";
    private const string ColorAcento = "#F7C484";
    private const string ColorExito = "#9FD996";
    private const string ColorAdvertencia = "#F3C95A";
    private const string ColorPeligro = "#EF5350";
    private const string ColorInfo = "#64B5F6";
    private const string ColorFondo = "#FEFEFD";
    private const string ColorTextoClaro = "#FEFEFD";

    public ReportesMotorService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera un reporte PDF completo del motor de inferencia con dashboard
    /// </summary>
    public async Task<byte[]> GenerarReportePdfAsync(DateOnly? fechaCorte = null, int? programaId = null)
    {
        fechaCorte ??= DateOnly.FromDateTime(DateTime.Now);

        var datos = await CargarDatosReporteAsync(fechaCorte.Value, programaId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposeHeader(c, datos, fechaCorte.Value));
                page.Content().Element(c => ComposeContent(c, datos));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private async Task<DatosReporteMotor> CargarDatosReporteAsync(DateOnly fechaCorte, int? programaId)
    {
        var datos = new DatosReporteMotor();
        var fechaCorteDateTime = fechaCorte.ToDateTime(TimeOnly.MaxValue);

        // Query base
        var queryAlertas = _context.Alertas
            .Include(a => a.Programa)
            .Include(a => a.Regla)
            .Where(a => !a.IsDeleted && a.GeneradaEn <= fechaCorteDateTime);

        if (programaId.HasValue)
        {
            queryAlertas = queryAlertas.Where(a => a.ProgramaId == programaId.Value);
            datos.Programa = await _context.Programas.FirstOrDefaultAsync(p => p.ProgramaId == programaId && !p.IsDeleted);
        }

        // Alertas
        datos.Alertas = await queryAlertas
            .OrderByDescending(a => a.GeneradaEn)
            .Select(a => new AlertaReporte
            {
                AlertaId = a.AlertaId,
                Mensaje = a.Mensaje,
                Severidad = a.Severidad.ToString(),
                SeveridadByte = (byte)a.Severidad,
                Estado = a.Estado.ToString(),
                EstadoByte = (byte)a.Estado,
                GeneradaEn = a.GeneradaEn,
                ResueltaEn = a.ResueltaEn,
                ProgramaNombre = a.Programa != null ? a.Programa.Nombre : null,
                ReglaNombre = a.Regla != null ? a.Regla.Nombre : null
            })
            .ToListAsync();

        // Estadísticas generales
        datos.TotalAlertas = datos.Alertas.Count;
        datos.AlertasAbiertas = datos.Alertas.Count(a => a.Estado == "Abierta");
        datos.AlertasResueltas = datos.Alertas.Count(a => a.Estado == "Resuelta");
        datos.AlertasDescartadas = datos.Alertas.Count(a => a.Estado == "Descartada");
        datos.AlertasCriticas = datos.Alertas.Count(a => a.Severidad == "Critica" && a.Estado == "Abierta");
        datos.AlertasAltas = datos.Alertas.Count(a => a.Severidad == "Alta" && a.Estado == "Abierta");
        datos.AlertasInfo = datos.Alertas.Count(a => a.Severidad == "Info");

        // Tasa de resolución
        datos.TasaResolucion = datos.TotalAlertas > 0 
            ? (datos.AlertasResueltas * 100.0 / datos.TotalAlertas) 
            : 0;

        // Tiempo promedio de resolución
        var alertasConResolucion = datos.Alertas.Where(a => a.ResueltaEn.HasValue).ToList();
        datos.TiempoPromedioResolucion = alertasConResolucion.Any()
            ? alertasConResolucion.Average(a => (a.ResueltaEn!.Value - a.GeneradaEn).TotalHours)
            : 0;

        // Alertas hoy y semana
        var hoy = DateTime.UtcNow.Date;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
        datos.AlertasHoy = datos.Alertas.Count(a => a.GeneradaEn >= hoy);
        datos.AlertasSemana = datos.Alertas.Count(a => a.GeneradaEn >= inicioSemana);

        // Reglas activas
        datos.ReglasActivas = await _context.Reglas.CountAsync(r => r.Activa && !r.IsDeleted);

        // Programas monitoreados
        if (programaId.HasValue)
        {
            datos.ProgramasMonitoreados = 1;
        }
        else
        {
            datos.ProgramasMonitoreados = await _context.Programas.CountAsync(p => !p.IsDeleted);
        }

        // Última ejecución
        datos.UltimaEjecucion = await _context.EjecucionesMotor
            .OrderByDescending(e => e.InicioUtc)
            .FirstOrDefaultAsync();

        // Alertas por programa
        datos.AlertasPorPrograma = datos.Alertas
            .Where(a => !string.IsNullOrEmpty(a.ProgramaNombre))
            .GroupBy(a => a.ProgramaNombre!)
            .Select(g => new AlertaPorPrograma
            {
                NombrePrograma = g.Key,
                Total = g.Count(),
                Criticas = g.Count(a => a.Severidad == "Critica" && a.Estado == "Abierta"),
                Altas = g.Count(a => a.Severidad == "Alta" && a.Estado == "Abierta"),
                Abiertas = g.Count(a => a.Estado == "Abierta")
            })
            .OrderByDescending(p => p.Criticas)
            .ThenByDescending(p => p.Total)
            .ToList();

        // Alertas por regla
        datos.AlertasPorRegla = datos.Alertas
            .Where(a => !string.IsNullOrEmpty(a.ReglaNombre))
            .GroupBy(a => a.ReglaNombre!)
            .Select(g => new AlertaPorRegla
            {
                NombreRegla = g.Key,
                Total = g.Count(),
                Abiertas = g.Count(a => a.Estado == "Abierta")
            })
            .OrderByDescending(r => r.Total)
            .Take(10)
            .ToList();

        return datos;
    }

    private void ComposeHeader(IContainer container, DatosReporteMotor datos, DateOnly fechaCorte)
    {
        container.Column(column =>
        {
            // Banner principal
            column.Item().Background(ColorPrimario).Padding(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Motor de Inferencia")
                        .FontSize(22)
                        .Bold()
                        .FontColor(ColorTextoClaro);

                    var subtitulo = $"Reporte de Alertas y Análisis - {fechaCorte:dd/MM/yyyy}";
                    if (datos.Programa != null)
                    {
                        subtitulo = $"Programa: {datos.Programa.Nombre} - {fechaCorte:dd/MM/yyyy}";
                    }

                    col.Item().PaddingTop(8).Text(subtitulo)
                        .FontSize(12)
                        .FontColor(ColorTextoClaro);

                    col.Item().PaddingTop(4).Text("Sistema de detección de riesgos y alertas")
                        .FontSize(10)
                        .FontColor(ColorTextoClaro)
                        .Italic();
                });

                row.ConstantItem(120).Column(col =>
                {
                    col.Item().AlignRight().Text("ONG")
                        .FontSize(12)
                        .Bold()
                        .FontColor(ColorExito);
                    col.Item().AlignRight().Text("Juventud")
                        .FontSize(14)
                        .Bold()
                        .FontColor(ColorTextoClaro);
                    col.Item().AlignRight().Text("Sin Límites")
                        .FontSize(14)
                        .Bold()
                        .FontColor(ColorTextoClaro);
                });
            });

            column.Item().Height(15);
        });
    }

    private void ComposeContent(IContainer container, DatosReporteMotor datos)
    {
        container.Column(column =>
        {
            // Resumen Ejecutivo
            column.Item().Element(c => ComposeResumenEjecutivo(c, datos));
            column.Item().Height(20);

            // KPIs Dashboard
            column.Item().Element(c => ComposeKPIsDashboard(c, datos));
            column.Item().Height(20);

            // Distribución por Severidad y Estado
            column.Item().Element(c => ComposeDistribuciones(c, datos));
            column.Item().Height(20);

            // Alertas por Programa
            if (datos.AlertasPorPrograma.Any())
            {
                column.Item().Element(c => ComposeAlertasPorPrograma(c, datos));
                column.Item().Height(20);
            }

            // Top Reglas
            if (datos.AlertasPorRegla.Any())
            {
                column.Item().Element(c => ComposeTopReglas(c, datos));
                column.Item().Height(20);
            }

            // Detalle de Alertas
            column.Item().Element(c => ComposeDetalleAlertas(c, datos));
        });
    }

    private void ComposeResumenEjecutivo(IContainer container, DatosReporteMotor datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorExito);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text("📊 RESUMEN EJECUTIVO")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Border(1).BorderColor("#E0E0E0").Padding(15).Column(col =>
            {
                col.Item().Text("El motor de inferencia ha analizado los programas y generado el siguiente resumen:")
                    .FontSize(10)
                    .LineHeight(1.5f);

                col.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("• Total de alertas: ").Bold().FontSize(10);
                        c.Item().Text("• Alertas abiertas: ").Bold().FontSize(10);
                        c.Item().Text("• Tasa de resolución: ").Bold().FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"{datos.TotalAlertas}").FontSize(10);
                        c.Item().Text($"{datos.AlertasAbiertas}").FontSize(10);
                        c.Item().Text($"{datos.TasaResolucion:F1}%").FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("• Alertas críticas: ").Bold().FontSize(10);
                        c.Item().Text("• Tiempo prom. resolución: ").Bold().FontSize(10);
                        c.Item().Text("• Programas monitoreados: ").Bold().FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"{datos.AlertasCriticas}").FontSize(10);
                        c.Item().Text($"{datos.TiempoPromedioResolucion:F1}h").FontSize(10);
                        c.Item().Text($"{datos.ProgramasMonitoreados}").FontSize(10);
                    });
                });

                if (datos.AlertasCriticas > 0)
                {
                    col.Item().PaddingTop(10).Background("#FFEBEE").Padding(8).Row(r =>
                    {
                        r.ConstantItem(20).AlignMiddle().Text("⚠️").FontSize(14);
                        r.RelativeItem().Text($"ATENCIÓN: Hay {datos.AlertasCriticas} alertas críticas que requieren atención inmediata.")
                            .FontSize(10)
                            .Bold()
                            .FontColor(ColorPeligro);
                    });
                }
            });
        });
    }

    private void ComposeKPIsDashboard(IContainer container, DatosReporteMotor datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorAcento);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text("📈 INDICADORES CLAVE (KPIs)")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Row(row =>
            {
                // KPI 1: Total Alertas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "📊 Total", 
                    datos.TotalAlertas.ToString(), 
                    "alertas generadas",
                    ColorInfo));
                
                row.ConstantItem(8);

                // KPI 2: Abiertas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "⚠️ Abiertas", 
                    datos.AlertasAbiertas.ToString(), 
                    "pendientes",
                    ColorAdvertencia));
                
                row.ConstantItem(8);

                // KPI 3: Resueltas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "✅ Resueltas", 
                    datos.AlertasResueltas.ToString(), 
                    "completadas",
                    ColorExito));
                
                row.ConstantItem(8);

                // KPI 4: Críticas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "🔴 Críticas", 
                    datos.AlertasCriticas.ToString(), 
                    "urgentes",
                    ColorPeligro));
                
                row.ConstantItem(8);

                // KPI 5: Tasa Resolución
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "📈 Resolución", 
                    $"{datos.TasaResolucion:F0}%", 
                    "efectividad",
                    ColorPrimario));
                
                row.ConstantItem(8);

                // KPI 6: Reglas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "📋 Reglas", 
                    datos.ReglasActivas.ToString(), 
                    "activas",
                    "#AB47BC"));
            });

            // Segunda fila de KPIs
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Padding(10).Column(c =>
                {
                    c.Item().Text("⏱️ Última Ejecución").FontSize(9).Bold().FontColor(ColorSecundario);
                    c.Item().PaddingTop(5).Text(datos.UltimaEjecucion?.InicioUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "N/A")
                        .FontSize(12).Bold().FontColor(ColorPrimario);
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Padding(10).Column(c =>
                {
                    c.Item().Text("📅 Alertas Hoy").FontSize(9).Bold().FontColor(ColorSecundario);
                    c.Item().PaddingTop(5).Text(datos.AlertasHoy.ToString())
                        .FontSize(12).Bold().FontColor(ColorPrimario);
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Padding(10).Column(c =>
                {
                    c.Item().Text("📆 Alertas Semana").FontSize(9).Bold().FontColor(ColorSecundario);
                    c.Item().PaddingTop(5).Text(datos.AlertasSemana.ToString())
                        .FontSize(12).Bold().FontColor(ColorPrimario);
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Padding(10).Column(c =>
                {
                    c.Item().Text("⏳ Tiempo Prom.").FontSize(9).Bold().FontColor(ColorSecundario);
                    c.Item().PaddingTop(5).Text($"{datos.TiempoPromedioResolucion:F1}h")
                        .FontSize(12).Bold().FontColor(ColorPrimario);
                });
            });
        });
    }

    private void ComposeKPICard(IContainer container, string titulo, string valor, string subtitulo, string color)
    {
        container.Border(1).BorderColor("#E0E0E0").Column(col =>
        {
            col.Item().Background(color).Padding(6).Text(titulo)
                .FontSize(8)
                .Bold()
                .FontColor(ColorTextoClaro);
            
            col.Item().Background(ColorFondo).Padding(8).Column(inner =>
            {
                inner.Item().AlignCenter().Text(valor)
                    .FontSize(20)
                    .Bold()
                    .FontColor(ColorPrimario);
                inner.Item().AlignCenter().Text(subtitulo)
                    .FontSize(7)
                    .FontColor(ColorSecundario);
            });
        });
    }

    private void ComposeDistribuciones(IContainer container, DatosReporteMotor datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorInfo);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text("📋 DISTRIBUCIÓN DE ALERTAS")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Row(row =>
            {
                // Por Severidad
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Column(col =>
                {
                    col.Item().Background(ColorSecundario).Padding(8).Text("Por Severidad")
                        .FontSize(10)
                        .Bold()
                        .FontColor(ColorTextoClaro);

                    col.Item().Padding(10).Column(inner =>
                    {
                        ComposeSeveridadItem(inner, "🔴 Crítica", datos.Alertas.Count(a => a.Severidad == "Critica"), datos.TotalAlertas, ColorPeligro);
                        ComposeSeveridadItem(inner, "🟠 Alta", datos.Alertas.Count(a => a.Severidad == "Alta"), datos.TotalAlertas, ColorAdvertencia);
                        ComposeSeveridadItem(inner, "🟢 Info", datos.Alertas.Count(a => a.Severidad == "Info"), datos.TotalAlertas, ColorExito);
                    });
                });

                row.ConstantItem(15);

                // Por Estado
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Column(col =>
                {
                    col.Item().Background(ColorSecundario).Padding(8).Text("Por Estado")
                        .FontSize(10)
                        .Bold()
                        .FontColor(ColorTextoClaro);

                    col.Item().Padding(10).Column(inner =>
                    {
                        ComposeEstadoItem(inner, "⚠️ Abiertas", datos.AlertasAbiertas, datos.TotalAlertas, ColorAdvertencia);
                        ComposeEstadoItem(inner, "✅ Resueltas", datos.AlertasResueltas, datos.TotalAlertas, ColorExito);
                        ComposeEstadoItem(inner, "❌ Descartadas", datos.AlertasDescartadas, datos.TotalAlertas, ColorSecundario);
                    });
                });
            });
        });
    }

    private void ComposeSeveridadItem(ColumnDescriptor column, string label, int cantidad, int total, string color)
    {
        var porcentaje = total > 0 ? (cantidad * 100.0 / total) : 0;
        
        column.Item().PaddingBottom(8).Column(c =>
        {
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(label).FontSize(9).Bold();
                r.ConstantItem(60).AlignRight().Text($"{cantidad} ({porcentaje:F0}%)").FontSize(9);
            });
            c.Item().PaddingTop(3).Height(8).Background("#E0E0E0").Row(bar =>
            {
                if (porcentaje > 0)
                {
                    bar.RelativeItem((float)porcentaje).Background(color);
                    bar.RelativeItem((float)(100 - porcentaje));
                }
            });
        });
    }

    private void ComposeEstadoItem(ColumnDescriptor column, string label, int cantidad, int total, string color)
    {
        var porcentaje = total > 0 ? (cantidad * 100.0 / total) : 0;
        
        column.Item().PaddingBottom(8).Row(r =>
        {
            r.ConstantItem(8).Height(8).Background(color);
            r.ConstantItem(5);
            r.RelativeItem().Text(label).FontSize(9);
            r.ConstantItem(70).AlignRight().Text($"{cantidad} ({porcentaje:F0}%)").FontSize(9).Bold();
        });
    }

    private void ComposeAlertasPorPrograma(IContainer container, DatosReporteMotor datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background("#AB47BC");
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text("📁 ALERTAS POR PROGRAMA")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(60);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(ColorPrimario).Padding(6).Text("Programa").FontSize(9).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Total").FontSize(9).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Críticas").FontSize(9).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Altas").FontSize(9).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Abiertas").FontSize(9).Bold().FontColor(ColorTextoClaro);
                });

                var isAlternate = false;
                foreach (var prog in datos.AlertasPorPrograma.Take(10))
                {
                    var bgColor = isAlternate ? "#F5F5F5" : ColorFondo;

                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).Text(TruncateText(prog.NombrePrograma, 30)).FontSize(8);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(prog.Total.ToString()).FontSize(8).Bold();
                    table.Cell().Background(prog.Criticas > 0 ? "#FFEBEE" : bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter()
                        .Text(prog.Criticas.ToString()).FontSize(8).Bold().FontColor(prog.Criticas > 0 ? ColorPeligro : ColorPrimario);
                    table.Cell().Background(prog.Altas > 0 ? "#FFF3E0" : bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter()
                        .Text(prog.Altas.ToString()).FontSize(8).FontColor(prog.Altas > 0 ? "#E65100" : ColorPrimario);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(prog.Abiertas.ToString()).FontSize(8);

                    isAlternate = !isAlternate;
                }
            });
        });
    }

    private void ComposeTopReglas(IContainer container, DatosReporteMotor datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorAdvertencia);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text("📋 TOP 10 REGLAS CON MÁS ALERTAS")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("#").FontSize(9).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).Text("Regla").FontSize(9).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Total").FontSize(9).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Abiertas").FontSize(9).Bold().FontColor(ColorTextoClaro);
                });

                var index = 1;
                var isAlternate = false;
                foreach (var regla in datos.AlertasPorRegla)
                {
                    var bgColor = isAlternate ? "#F5F5F5" : ColorFondo;

                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(index.ToString()).FontSize(8).Bold();
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).Text(TruncateText(regla.NombreRegla, 40)).FontSize(8);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(regla.Total.ToString()).FontSize(8).Bold();
                    table.Cell().Background(regla.Abiertas > 0 ? "#FFF3E0" : bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter()
                        .Text(regla.Abiertas.ToString()).FontSize(8).FontColor(regla.Abiertas > 0 ? "#E65100" : ColorPrimario);

                    index++;
                    isAlternate = !isAlternate;
                }
            });
        });
    }

    private void ComposeDetalleAlertas(IContainer container, DatosReporteMotor datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorPeligro);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text($"📝 DETALLE DE ALERTAS ({datos.Alertas.Count})")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.ConstantColumn(55);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(55);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(ColorPrimario).Padding(5).AlignCenter().Text("ID").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(5).Text("Severidad").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(5).Text("Mensaje").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(5).Text("Programa").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(5).AlignCenter().Text("Fecha").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(5).AlignCenter().Text("Estado").FontSize(8).Bold().FontColor(ColorTextoClaro);
                });

                var isAlternate = false;
                foreach (var alerta in datos.Alertas.Take(50))
                {
                    var bgColor = isAlternate ? "#F5F5F5" : ColorFondo;
                    var severidadColor = alerta.Severidad switch
                    {
                        "Critica" => ColorPeligro,
                        "Alta" => "#E65100",
                        _ => ColorExito
                    };
                    var estadoColor = alerta.Estado switch
                    {
                        "Abierta" => ColorAdvertencia,
                        "Resuelta" => ColorExito,
                        _ => ColorSecundario
                    };

                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).AlignCenter().Text(alerta.AlertaId.ToString()).FontSize(7);
                    table.Cell().Background(severidadColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(alerta.Severidad).FontSize(7).Bold().FontColor(ColorTextoClaro);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(TruncateText(alerta.Mensaje, 45)).FontSize(7);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(TruncateText(alerta.ProgramaNombre ?? "-", 15)).FontSize(7);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).AlignCenter().Text(alerta.GeneradaEn.ToString("dd/MM/yy")).FontSize(7);
                    table.Cell().Background(estadoColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).AlignCenter().Text(alerta.Estado).FontSize(7).Bold().FontColor(ColorTextoClaro);

                    isAlternate = !isAlternate;
                }
            });

            if (datos.Alertas.Count > 50)
            {
                column.Item().PaddingTop(5).Text($"... y {datos.Alertas.Count - 50} alertas más")
                    .FontSize(8)
                    .FontColor(ColorSecundario)
                    .Italic();
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("Motor de Inferencia - ONG Juventud Sin Límites")
                    .FontSize(7)
                    .FontColor(ColorSecundario);
                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.Span("Página ").FontSize(7).FontColor(ColorSecundario);
                    text.CurrentPageNumber().FontSize(7).FontColor(ColorSecundario);
                    text.Span(" de ").FontSize(7).FontColor(ColorSecundario);
                    text.TotalPages().FontSize(7).FontColor(ColorSecundario);
                });
                row.RelativeItem().AlignRight().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(7)
                    .FontColor(ColorSecundario);
            });
        });
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return text.Substring(0, maxLength) + "...";
    }

    // Clases auxiliares
    private class DatosReporteMotor
    {
        public Domain.Programas.Programa? Programa { get; set; }
        public List<AlertaReporte> Alertas { get; set; } = new();
        public int TotalAlertas { get; set; }
        public int AlertasAbiertas { get; set; }
        public int AlertasResueltas { get; set; }
        public int AlertasDescartadas { get; set; }
        public int AlertasCriticas { get; set; }
        public int AlertasAltas { get; set; }
        public int AlertasInfo { get; set; }
        public double TasaResolucion { get; set; }
        public double TiempoPromedioResolucion { get; set; }
        public int AlertasHoy { get; set; }
        public int AlertasSemana { get; set; }
        public int ReglasActivas { get; set; }
        public int ProgramasMonitoreados { get; set; }
        public Domain.Motor.EjecucionMotor? UltimaEjecucion { get; set; }
        public List<AlertaPorPrograma> AlertasPorPrograma { get; set; } = new();
        public List<AlertaPorRegla> AlertasPorRegla { get; set; } = new();
    }

    private class AlertaReporte
    {
        public int AlertaId { get; set; }
        public string Mensaje { get; set; } = "";
        public string Severidad { get; set; } = "";
        public byte SeveridadByte { get; set; }
        public string Estado { get; set; } = "";
        public byte EstadoByte { get; set; }
        public DateTime GeneradaEn { get; set; }
        public DateTime? ResueltaEn { get; set; }
        public string? ProgramaNombre { get; set; }
        public string? ReglaNombre { get; set; }
    }

    private class AlertaPorPrograma
    {
        public string NombrePrograma { get; set; } = "";
        public int Total { get; set; }
        public int Criticas { get; set; }
        public int Altas { get; set; }
        public int Abiertas { get; set; }
    }

    private class AlertaPorRegla
    {
        public string NombreRegla { get; set; } = "";
        public int Total { get; set; }
        public int Abiertas { get; set; }
    }
}
