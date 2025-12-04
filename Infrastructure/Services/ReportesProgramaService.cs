using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

/// <summary>
/// Servicio para generar reportes PDF de programas con dashboard completo
/// </summary>
public class ReportesProgramaService
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

    public ReportesProgramaService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera un reporte PDF detallado del programa con todos los datos del dashboard
    /// </summary>
    public async Task<byte[]> GenerarReportePdfAsync(int programaId, int anio, int? mes = null)
    {
        var programa = await _context.Programas
            .FirstOrDefaultAsync(p => p.ProgramaId == programaId && !p.IsDeleted);

        if (programa == null)
            throw new InvalidOperationException("Programa no encontrado");

        // Cargar todos los datos necesarios
        var datos = await CargarDatosReporteAsync(programaId, anio, mes);

        // Generar PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposeHeader(c, programa, anio, mes));
                page.Content().Element(c => ComposeContent(c, datos, programa, anio, mes));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private async Task<DatosReporte> CargarDatosReporteAsync(int programaId, int anio, int? mes)
    {
        var datos = new DatosReporte();

        // Query base para actividades
        var queryActividades = _context.Actividades
            .Where(a => a.ProgramaId == programaId && !a.IsDeleted && a.FechaInicio.Year == anio);
        
        if (mes.HasValue)
            queryActividades = queryActividades.Where(a => a.FechaInicio.Month == mes.Value);

        datos.Actividades = await queryActividades.OrderBy(a => a.FechaInicio).ToListAsync();

        // Métricas
        var prefixoAnioMes = mes.HasValue ? $"{anio:0000}-{mes:00}" : anio.ToString();
        datos.Metricas = await _context.MetricasProgramaMes
            .Where(m => m.ProgramaId == programaId && !m.IsDeleted && m.AnioMes.StartsWith(prefixoAnioMes))
            .OrderBy(m => m.AnioMes)
            .ToListAsync();

        // Alertas
        var queryAlertas = _context.Alertas
            .Include(a => a.Regla)
            .Where(a => a.ProgramaId == programaId && !a.IsDeleted && a.GeneradaEn.Year == anio);
        
        if (mes.HasValue)
            queryAlertas = queryAlertas.Where(a => a.GeneradaEn.Month == mes.Value);

        datos.Alertas = await queryAlertas.OrderByDescending(a => a.GeneradaEn).ToListAsync();

        // Participantes activos
        datos.ParticipantesActivos = await _context.ActividadParticipantes
            .Where(ap => ap.Actividad.ProgramaId == programaId && 
                        !ap.IsDeleted && 
                        ap.Estado == EstadoInscripcion.Inscrito)
            .Select(ap => ap.ParticipanteId)
            .Distinct()
            .CountAsync();

        // Total inscritos
        datos.TotalInscritos = await _context.ActividadParticipantes
            .Where(ap => ap.Actividad.ProgramaId == programaId && !ap.IsDeleted)
            .CountAsync();

        // Asistencias
        var queryAsistencias = _context.Asistencias
            .Where(a => a.Actividad.ProgramaId == programaId && !a.IsDeleted && a.Fecha.Year == anio);
        
        if (mes.HasValue)
            queryAsistencias = queryAsistencias.Where(a => a.Fecha.Month == mes.Value);

        var asistencias = await queryAsistencias.ToListAsync();
        datos.TotalAsistencias = asistencias.Count;
        datos.AsistenciasPresentes = asistencias.Count(a => a.Estado == EstadoAsistencia.Presente || a.Estado == EstadoAsistencia.Tarde);

        // Evidencias
        datos.TotalEvidencias = await _context.EvidenciaActividades
            .Where(e => e.Actividad.ProgramaId == programaId && !e.IsDeleted)
            .CountAsync();

        // Distribución por tipo
        datos.DistribucionPorTipo = datos.Actividades
            .GroupBy(a => a.Tipo)
            .Select(g => new TipoConteo { Tipo = g.Key.ToString(), Cantidad = g.Count() })
            .ToList();

        // Distribución por estado
        datos.DistribucionPorEstado = datos.Actividades
            .GroupBy(a => a.Estado)
            .Select(g => new EstadoConteo { Estado = g.Key.ToString(), Cantidad = g.Count() })
            .ToList();

        // Calcular métricas adicionales
        datos.ActividadesEjecutadas = datos.Actividades.Count(a => a.Estado == EstadoActividad.Realizada);
        datos.ActividadesPlanificadas = datos.Actividades.Count(a => a.Estado == EstadoActividad.Planificada);
        datos.ActividadesCanceladas = datos.Actividades.Count(a => a.Estado == EstadoActividad.Cancelada);

        // Horas de actividad
        datos.HorasActividad = datos.Actividades
            .Where(a => a.Estado == EstadoActividad.Realizada && a.FechaFin.HasValue)
            .Sum(a => (a.FechaFin!.Value - a.FechaInicio).TotalHours);

        // Días activos
        datos.DiasActivos = datos.Actividades
            .Where(a => a.Estado == EstadoActividad.Realizada)
            .Select(a => a.FechaInicio.Date)
            .Distinct()
            .Count();

        return datos;
    }

    private void ComposeHeader(IContainer container, Domain.Programas.Programa programa, int anio, int? mes)
    {
        container.Column(column =>
        {
            // Banner principal
            column.Item().Background(ColorPrimario).Padding(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(programa.Nombre)
                        .FontSize(22)
                        .Bold()
                        .FontColor(ColorTextoClaro);

                    col.Item().PaddingTop(8).Text($"Reporte de Gestión {(mes.HasValue ? ObtenerNombreMes(mes.Value) + " " : "")}{anio}")
                        .FontSize(14)
                        .FontColor(ColorTextoClaro);

                    col.Item().PaddingTop(4).Text(programa.Descripcion ?? "")
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
                    col.Item().PaddingTop(8).AlignRight().Text($"Clave: {programa.Clave}")
                        .FontSize(9)
                        .FontColor(ColorAcento);
                });
            });

            column.Item().Height(15);
        });
    }

    private void ComposeContent(IContainer container, DatosReporte datos, Domain.Programas.Programa programa, int anio, int? mes)
    {
        container.Column(column =>
        {
            // Resumen Ejecutivo
            column.Item().Element(c => ComposeResumenEjecutivo(c, datos, anio, mes));
            column.Item().Height(20);

            // KPIs Dashboard
            column.Item().Element(c => ComposeKPIsDashboard(c, datos));
            column.Item().Height(20);

            // Distribución y Estadísticas
            column.Item().Element(c => ComposeEstadisticas(c, datos));
            column.Item().Height(20);

            // Métricas Mensuales (si hay datos)
            if (datos.Metricas.Any())
            {
                column.Item().Element(c => ComposeMetricasMensuales(c, datos));
                column.Item().Height(20);
            }

            // Actividades
            column.Item().Element(c => ComposeActividadesDetalle(c, datos));
            column.Item().Height(20);

            // Alertas (si hay)
            if (datos.Alertas.Any())
            {
                column.Item().Element(c => ComposeAlertasResumen(c, datos));
            }
        });
    }

    private void ComposeResumenEjecutivo(IContainer container, DatosReporte datos, int anio, int? mes)
    {
        var cumplimiento = datos.Actividades.Count > 0 
            ? (datos.ActividadesEjecutadas * 100.0 / datos.Actividades.Count) 
            : 0;
        var asistencia = datos.TotalAsistencias > 0 
            ? (datos.AsistenciasPresentes * 100.0 / datos.TotalAsistencias) 
            : 0;

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
                col.Item().Text($"Durante {(mes.HasValue ? ObtenerNombreMes(mes.Value) + " de " : "el año ")}{anio}, el programa ha demostrado los siguientes resultados:")
                    .FontSize(10)
                    .LineHeight(1.5f);

                col.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("• Actividades totales: ").Bold().FontSize(10);
                        c.Item().Text("• Tasa de cumplimiento: ").Bold().FontSize(10);
                        c.Item().Text("• Participantes activos: ").Bold().FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"{datos.Actividades.Count}").FontSize(10);
                        c.Item().Text($"{cumplimiento:F1}%").FontSize(10);
                        c.Item().Text($"{datos.ParticipantesActivos}").FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("• Asistencia promedio: ").Bold().FontSize(10);
                        c.Item().Text("• Horas de actividad: ").Bold().FontSize(10);
                        c.Item().Text("• Alertas generadas: ").Bold().FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"{asistencia:F1}%").FontSize(10);
                        c.Item().Text($"{datos.HorasActividad:F0}h").FontSize(10);
                        c.Item().Text($"{datos.Alertas.Count}").FontSize(10);
                    });
                });
            });
        });
    }

    private void ComposeKPIsDashboard(IContainer container, DatosReporte datos)
    {
        var cumplimiento = datos.Actividades.Count > 0 
            ? (datos.ActividadesEjecutadas * 100.0 / datos.Actividades.Count) 
            : 0;
        var asistencia = datos.TotalAsistencias > 0 
            ? (datos.AsistenciasPresentes * 100.0 / datos.TotalAsistencias) 
            : 0;
        var alertasCriticas = datos.Alertas.Count(a => a.Severidad == Severidad.Critica);

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
                // KPI 1: Ejecutadas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "✅ Ejecutadas", 
                    datos.ActividadesEjecutadas.ToString(), 
                    $"de {datos.Actividades.Count} totales",
                    ColorExito));
                
                row.ConstantItem(10);

                // KPI 2: Planificadas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "📅 Planificadas", 
                    datos.ActividadesPlanificadas.ToString(), 
                    "pendientes",
                    ColorAcento));
                
                row.ConstantItem(10);

                // KPI 3: Participantes
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "👥 Participantes", 
                    datos.ParticipantesActivos.ToString(), 
                    "activos",
                    ColorInfo));
                
                row.ConstantItem(10);

                // KPI 4: Asistencia
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "✔️ Asistencia", 
                    $"{asistencia:F0}%", 
                    "promedio",
                    ColorPrimario));
                
                row.ConstantItem(10);

                // KPI 5: Cumplimiento
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "📊 Cumplimiento", 
                    $"{cumplimiento:F0}%", 
                    "del plan",
                    "#AB47BC"));
                
                row.ConstantItem(10);

                // KPI 6: Alertas
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "🔔 Alertas", 
                    datos.Alertas.Count.ToString(), 
                    $"{alertasCriticas} críticas",
                    alertasCriticas > 0 ? ColorPeligro : ColorExito));
            });
        });
    }

    private void ComposeKPICard(IContainer container, string titulo, string valor, string subtitulo, string color)
    {
        container.Border(1).BorderColor("#E0E0E0").Column(col =>
        {
            col.Item().Background(color).Padding(8).Text(titulo)
                .FontSize(8)
                .Bold()
                .FontColor(ColorTextoClaro);
            
            col.Item().Background(ColorFondo).Padding(10).Column(inner =>
            {
                inner.Item().AlignCenter().Text(valor)
                    .FontSize(22)
                    .Bold()
                    .FontColor(ColorPrimario);
                inner.Item().AlignCenter().Text(subtitulo)
                    .FontSize(8)
                    .FontColor(ColorSecundario);
            });
        });
    }

    private void ComposeEstadisticas(IContainer container, DatosReporte datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorInfo);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text("📋 DISTRIBUCIÓN Y ESTADÍSTICAS")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Row(row =>
            {
                // Distribución por Tipo
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Column(col =>
                {
                    col.Item().Background(ColorSecundario).Padding(8).Text("Por Tipo de Actividad")
                        .FontSize(10)
                        .Bold()
                        .FontColor(ColorTextoClaro);

                    col.Item().Padding(10).Column(inner =>
                    {
                        foreach (var tipo in datos.DistribucionPorTipo)
                        {
                            var porcentaje = datos.Actividades.Count > 0 
                                ? (tipo.Cantidad * 100.0 / datos.Actividades.Count) 
                                : 0;
                            
                            inner.Item().PaddingBottom(5).Row(r =>
                            {
                                r.RelativeItem(2).Text(tipo.Tipo).FontSize(9);
                                r.RelativeItem(1).AlignRight().Text($"{tipo.Cantidad}").FontSize(9).Bold();
                                r.RelativeItem(1).AlignRight().Text($"({porcentaje:F0}%)").FontSize(8).FontColor(ColorSecundario);
                            });
                        }

                        if (!datos.DistribucionPorTipo.Any())
                        {
                            inner.Item().Text("Sin datos").FontSize(9).FontColor(ColorSecundario).Italic();
                        }
                    });
                });

                row.ConstantItem(15);

                // Distribución por Estado
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Column(col =>
                {
                    col.Item().Background(ColorSecundario).Padding(8).Text("Por Estado")
                        .FontSize(10)
                        .Bold()
                        .FontColor(ColorTextoClaro);

                    col.Item().Padding(10).Column(inner =>
                    {
                        foreach (var estado in datos.DistribucionPorEstado)
                        {
                            var color = estado.Estado switch
                            {
                                "Realizada" => ColorExito,
                                "Planificada" => ColorAcento,
                                "Cancelada" => ColorPeligro,
                                _ => ColorSecundario
                            };

                            inner.Item().PaddingBottom(5).Row(r =>
                            {
                                r.ConstantItem(8).Height(8).Background(color);
                                r.ConstantItem(5);
                                r.RelativeItem(2).Text(estado.Estado).FontSize(9);
                                r.RelativeItem(1).AlignRight().Text($"{estado.Cantidad}").FontSize(9).Bold();
                            });
                        }

                        if (!datos.DistribucionPorEstado.Any())
                        {
                            inner.Item().Text("Sin datos").FontSize(9).FontColor(ColorSecundario).Italic();
                        }
                    });
                });

                row.ConstantItem(15);

                // Estadísticas Generales
                row.RelativeItem().Border(1).BorderColor("#E0E0E0").Column(col =>
                {
                    col.Item().Background(ColorSecundario).Padding(8).Text("Estadísticas Generales")
                        .FontSize(10)
                        .Bold()
                        .FontColor(ColorTextoClaro);

                    col.Item().Padding(10).Column(inner =>
                    {
                        ComposeEstadisticaItem(inner, "Total Inscritos", datos.TotalInscritos.ToString());
                        ComposeEstadisticaItem(inner, "Evidencias", datos.TotalEvidencias.ToString());
                        ComposeEstadisticaItem(inner, "Horas de Actividad", $"{datos.HorasActividad:F0}h");
                        ComposeEstadisticaItem(inner, "Días Activos", datos.DiasActivos.ToString());
                    });
                });
            });
        });
    }

    private void ComposeEstadisticaItem(ColumnDescriptor column, string label, string valor)
    {
        column.Item().PaddingBottom(5).Row(r =>
        {
            r.RelativeItem(2).Text(label).FontSize(9);
            r.RelativeItem(1).AlignRight().Text(valor).FontSize(9).Bold();
        });
    }

    private void ComposeMetricasMensuales(IContainer container, DatosReporte datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background("#AB47BC");
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text("📅 MÉTRICAS MENSUALES")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(ColorPrimario).Padding(6).Text("Período").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Planificadas").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Ejecutadas").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Cumplimiento").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Asistencia").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Retraso").FontSize(8).Bold().FontColor(ColorTextoClaro);
                });

                // Rows
                var isAlternate = false;
                foreach (var metrica in datos.Metricas)
                {
                    var bgColor = isAlternate ? "#F5F5F5" : ColorFondo;
                    var cumplimientoColor = metrica.PorcCumplimiento >= 80 ? ColorExito : 
                                           metrica.PorcCumplimiento >= 60 ? ColorAdvertencia : ColorPeligro;

                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).Text(FormatearPeriodo(metrica.AnioMes)).FontSize(8).Bold();
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(metrica.ActividadesPlanificadas.ToString()).FontSize(8);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(metrica.ActividadesEjecutadas.ToString()).FontSize(8);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text($"{metrica.PorcCumplimiento:F1}%").FontSize(8).FontColor(cumplimientoColor).Bold();
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text($"{metrica.PorcAsistenciaProm:F1}%").FontSize(8);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text($"{metrica.RetrasoPromedioDias:F1}d").FontSize(8);

                    isAlternate = !isAlternate;
                }
            });
        });
    }

    private void ComposeActividadesDetalle(IContainer container, DatosReporte datos)
    {
        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorExito);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text($"📝 DETALLE DE ACTIVIDADES ({datos.Actividades.Count})")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(65);
                    columns.RelativeColumn(2);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(ColorPrimario).Padding(6).Text("Título").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Fecha").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Tipo").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).AlignCenter().Text("Estado").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    header.Cell().Background(ColorPrimario).Padding(6).Text("Lugar").FontSize(8).Bold().FontColor(ColorTextoClaro);
                });

                // Rows (máximo 30 para no hacer el PDF muy largo)
                var isAlternate = false;
                foreach (var actividad in datos.Actividades.Take(30))
                {
                    var bgColor = isAlternate ? "#F5F5F5" : ColorFondo;
                    var estadoColor = actividad.Estado switch
                    {
                        EstadoActividad.Realizada => ColorExito,
                        EstadoActividad.Planificada => ColorAcento,
                        EstadoActividad.Cancelada => ColorPeligro,
                        _ => ColorSecundario
                    };

                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).Text(TruncateText(actividad.Titulo, 40)).FontSize(8);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(actividad.FechaInicio.ToString("dd/MM/yy HH:mm")).FontSize(7);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(actividad.Tipo.ToString()).FontSize(8);
                    table.Cell().Background(estadoColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).AlignCenter().Text(actividad.Estado.ToString()).FontSize(7).Bold().FontColor(ColorTextoClaro);
                    table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(5).Text(TruncateText(actividad.Lugar ?? "N/A", 25)).FontSize(8);

                    isAlternate = !isAlternate;
                }
            });

            if (datos.Actividades.Count > 30)
            {
                column.Item().PaddingTop(5).Text($"... y {datos.Actividades.Count - 30} actividades más")
                    .FontSize(8)
                    .FontColor(ColorSecundario)
                    .Italic();
            }
        });
    }

    private void ComposeAlertasResumen(IContainer container, DatosReporte datos)
    {
        var alertasCriticas = datos.Alertas.Count(a => a.Severidad == Severidad.Critica);
        var alertasAltas = datos.Alertas.Count(a => a.Severidad == Severidad.Alta);
        var alertasInfo = datos.Alertas.Count(a => a.Severidad == Severidad.Info);

        container.Column(column =>
        {
            // Título de sección
            column.Item().Row(row =>
            {
                row.ConstantItem(5).Background(ColorPeligro);
                row.RelativeItem().Padding(10).Background("#F5F5F5").Text($"⚠️ ALERTAS GENERADAS ({datos.Alertas.Count})")
                    .FontSize(12)
                    .Bold()
                    .FontColor(ColorPrimario);
            });

            // Resumen por severidad
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(ColorPeligro).Padding(8).Column(c =>
                {
                    c.Item().Text("Críticas").FontSize(8).Bold().FontColor(ColorPeligro);
                    c.Item().AlignCenter().Text(alertasCriticas.ToString()).FontSize(18).Bold().FontColor(ColorPrimario);
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor(ColorAcento).Padding(8).Column(c =>
                {
                    c.Item().Text("Altas").FontSize(8).Bold().FontColor(ColorAcento);
                    c.Item().AlignCenter().Text(alertasAltas.ToString()).FontSize(18).Bold().FontColor(ColorPrimario);
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor(ColorInfo).Padding(8).Column(c =>
                {
                    c.Item().Text("Informativas").FontSize(8).Bold().FontColor(ColorInfo);
                    c.Item().AlignCenter().Text(alertasInfo.ToString()).FontSize(18).Bold().FontColor(ColorPrimario);
                });
            });

            // Tabla de alertas recientes (máximo 15)
            if (datos.Alertas.Any())
            {
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(55);
                        columns.ConstantColumn(90);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(ColorPrimario).Padding(5).Text("Fecha").FontSize(8).Bold().FontColor(ColorTextoClaro);
                        header.Cell().Background(ColorPrimario).Padding(5).Text("Severidad").FontSize(8).Bold().FontColor(ColorTextoClaro);
                        header.Cell().Background(ColorPrimario).Padding(5).Text("Regla").FontSize(8).Bold().FontColor(ColorTextoClaro);
                        header.Cell().Background(ColorPrimario).Padding(5).Text("Mensaje").FontSize(8).Bold().FontColor(ColorTextoClaro);
                        header.Cell().Background(ColorPrimario).Padding(5).Text("Estado").FontSize(8).Bold().FontColor(ColorTextoClaro);
                    });

                    var isAlternate = false;
                    foreach (var alerta in datos.Alertas.Take(15))
                    {
                        var bgColor = isAlternate ? "#F5F5F5" : ColorFondo;
                        var severidadColor = alerta.Severidad switch
                        {
                            Severidad.Critica => ColorPeligro,
                            Severidad.Alta => ColorAcento,
                            _ => ColorInfo
                        };

                        table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(alerta.GeneradaEn.ToString("dd/MM/yy")).FontSize(7);
                        table.Cell().Background(severidadColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(alerta.Severidad.ToString()).FontSize(7).Bold().FontColor(ColorTextoClaro);
                        table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(TruncateText(alerta.Regla?.Nombre ?? "N/A", 15)).FontSize(7);
                        table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(TruncateText(alerta.Mensaje, 50)).FontSize(7);
                        table.Cell().Background(bgColor).Border(0.5f).BorderColor("#E0E0E0").Padding(4).Text(alerta.Estado.ToString()).FontSize(7);

                        isAlternate = !isAlternate;
                    }
                });
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
                row.RelativeItem().Text($"Sistema Experto ONG - Juventud Sin Límites")
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

    // Métodos auxiliares
    private string ObtenerNombreMes(int mes) => mes switch
    {
        1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
        5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
        9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
        _ => ""
    };

    private string FormatearPeriodo(string anioMes)
    {
        if (anioMes.Length == 7) // YYYY-MM
        {
            var partes = anioMes.Split('-');
            if (partes.Length == 2 && int.TryParse(partes[1], out int mes))
            {
                return $"{ObtenerNombreMes(mes).Substring(0, 3)} {partes[0]}";
            }
        }
        return anioMes;
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return text.Substring(0, maxLength) + "...";
    }

    // Clases auxiliares para datos del reporte
    private class DatosReporte
    {
        public List<Domain.Operacion.Actividad> Actividades { get; set; } = new();
        public List<Domain.BI.MetricasProgramaMes> Metricas { get; set; } = new();
        public List<Domain.Motor.Alerta> Alertas { get; set; } = new();
        public int ParticipantesActivos { get; set; }
        public int TotalInscritos { get; set; }
        public int TotalAsistencias { get; set; }
        public int AsistenciasPresentes { get; set; }
        public int TotalEvidencias { get; set; }
        public int ActividadesEjecutadas { get; set; }
        public int ActividadesPlanificadas { get; set; }
        public int ActividadesCanceladas { get; set; }
        public double HorasActividad { get; set; }
        public int DiasActivos { get; set; }
        public List<TipoConteo> DistribucionPorTipo { get; set; } = new();
        public List<EstadoConteo> DistribucionPorEstado { get; set; } = new();
    }

    private class TipoConteo
    {
        public string Tipo { get; set; } = "";
        public int Cantidad { get; set; }
    }

    private class EstadoConteo
    {
        public string Estado { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
