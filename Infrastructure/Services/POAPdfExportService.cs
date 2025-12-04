using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Pages.POA;
using System.IO;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

/// <summary>
/// Servicio para exportar POA a PDF con diseño profesional
/// </summary>
public class POAPdfExportService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly POAService _poaService;

    // Colores del tema Mythra
    private static readonly string ColorPrimario = "#4D3935";
    private static readonly string ColorSecundario = "#6D534F";
    private static readonly string ColorVerde = "#9FD996";
    private static readonly string ColorAmarillo = "#F7C484";
    private static readonly string ColorDorado = "#F3C95A";
    private static readonly string ColorFondo = "#F7F7F7";
    private static readonly string ColorTextoClaro = "#FEFEFD";

    public POAPdfExportService(ApplicationDbContext context, IWebHostEnvironment environment, POAService poaService)
    {
        _context = context;
        _environment = environment;
        _poaService = poaService;
        
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera un PDF del POA completo con dashboard y métricas
    /// </summary>
    public async Task<byte[]> GenerarPOAPdfAsync(int instanciaId)
    {
        var instancia = await _context.POAInstancias
            .Include(i => i.Programa)
            .Include(i => i.Plantilla)
                .ThenInclude(p => p!.Campos.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Seccion)
            .FirstOrDefaultAsync(i => i.InstanciaId == instanciaId && !i.IsDeleted);

        if (instancia == null)
            throw new InvalidOperationException($"No se encontró la instancia POA con ID {instanciaId}");

        var valores = await _context.POAValores
            .Include(v => v.Campo)
            .Where(v => v.InstanciaId == instanciaId && !v.IsDeleted)
            .ToListAsync();

        var metricas = await _poaService.CalcularMetricasAsync(instanciaId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, instancia));
                page.Content().Element(c => ComposeFullContent(c, instancia, valores, metricas));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Genera un PDF resumen ejecutivo
    /// </summary>
    public async Task<byte[]> GenerarResumenPOAPdfAsync(int instanciaId)
    {
        var instancia = await _context.POAInstancias
            .Include(i => i.Programa)
            .Include(i => i.Plantilla)
            .FirstOrDefaultAsync(i => i.InstanciaId == instanciaId && !i.IsDeleted);

        if (instancia == null)
            throw new InvalidOperationException($"No se encontró la instancia POA con ID {instanciaId}");

        var metricas = await _poaService.CalcularMetricasAsync(instanciaId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, instancia));
                page.Content().Element(c => ComposeResumenEjecutivo(c, instancia, metricas));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    #region Composición del Header

    private void ComposeHeader(IContainer container, POAInstancia instancia)
    {
        container.Column(column =>
        {
            // Barra superior con gradiente simulado
            column.Item().Background(ColorPrimario).Padding(15).Row(row =>
            {
                // Logo del programa
                row.ConstantItem(80).Height(60).Element(c =>
                {
                    var logoPath = ObtenerRutaLogo(instancia.Programa?.Clave ?? "");
                    if (File.Exists(logoPath))
                    {
                        c.AlignCenter().AlignMiddle().Image(logoPath, ImageScaling.FitArea);
                    }
                    else
                    {
                        c.Background(ColorVerde).AlignCenter().AlignMiddle()
                            .Text(instancia.Programa?.Clave ?? "POA")
                            .FontSize(14).Bold().FontColor(ColorPrimario);
                    }
                });

                // Título e información
                row.RelativeItem().PaddingLeft(15).Column(col =>
                {
                    col.Item().Text("PLAN OPERATIVO ANUAL")
                        .FontSize(20).Bold().FontColor(ColorTextoClaro);

                    col.Item().PaddingTop(5).Text($"{instancia.Programa?.Nombre ?? "N/A"}")
                        .FontSize(14).SemiBold().FontColor(ColorAmarillo);

                    col.Item().PaddingTop(3).Row(infoRow =>
                    {
                        infoRow.AutoItem().Text($"Periodo: {FormatearPeriodo(instancia)}")
                            .FontSize(10).FontColor(ColorTextoClaro);
                        infoRow.AutoItem().PaddingHorizontal(10).Text("|").FontColor(ColorTextoClaro);
                        infoRow.AutoItem().Text($"Estado: {ObtenerNombreEstado(instancia.Estado)}")
                            .FontSize(10).Bold().FontColor(ObtenerColorEstadoPdf(instancia.Estado));
                    });
                });

                // Logo ONG
                row.ConstantItem(80).Height(60).AlignCenter().AlignMiddle().Column(col =>
                {
                    col.Item().AlignCenter().Text("ONG")
                        .FontSize(12).Bold().FontColor(ColorVerde);
                    col.Item().AlignCenter().Text("Juventud")
                        .FontSize(9).FontColor(ColorTextoClaro);
                    col.Item().AlignCenter().Text("Sin Límites")
                        .FontSize(9).FontColor(ColorTextoClaro);
                });
            });

            // Línea decorativa
            column.Item().Height(4).Background(ColorVerde);
        });
    }

    #endregion

    #region Contenido Completo

    private void ComposeFullContent(IContainer container, POAInstancia instancia, List<POAValor> valores, MetricasPOAViewModel metricas)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            // Dashboard KPIs
            column.Item().Element(c => ComposeDashboardKPIs(c, metricas));

            // Gráficos visuales
            column.Item().Element(c => ComposeGraficosVisuales(c, metricas));

            // Detalle de Actividades y Presupuesto
            column.Item().Element(c => ComposeDetalleOperativo(c, metricas));

            // Impacto Social
            column.Item().Element(c => ComposeImpactoSocial(c, metricas));

            // Recursos Humanos y Alianzas
            column.Item().Element(c => ComposeRecursosYAlianzas(c, metricas));

            // Análisis Cualitativo
            column.Item().Element(c => ComposeAnalisisCualitativo(c, metricas));

            // Notas adicionales
            if (!string.IsNullOrWhiteSpace(instancia.Notas))
            {
                column.Item().Element(c => ComposeNotas(c, instancia.Notas));
            }
        });
    }

    private void ComposeDashboardKPIs(IContainer container, MetricasPOAViewModel metricas)
    {
        container.Column(column =>
        {
            column.Item().Text("INDICADORES CLAVE DE DESEMPEÑO")
                .FontSize(12).Bold().FontColor(ColorPrimario);

            column.Item().PaddingTop(10).Row(row =>
            {
                // KPI 1: Presupuesto
                row.RelativeItem().Element(c => ComposeKPICard(c, 
                    "💰 Presupuesto Total",
                    metricas.PresupuestoTotal.ToString("C0"),
                    $"Ejecutado: {metricas.PresupuestoEjecutado:C0}",
                    metricas.PresupuestoTotal > 0 ? (double)(metricas.PresupuestoEjecutado * 100 / metricas.PresupuestoTotal) : 0,
                    ColorVerde));

                row.ConstantItem(10);

                // KPI 2: Participantes
                row.RelativeItem().Element(c => ComposeKPICard(c,
                    "👥 Participantes",
                    metricas.TotalParticipantes.ToString("N0"),
                    $"Activos: {metricas.ParticipantesActivos}",
                    metricas.TotalParticipantes > 0 ? (double)metricas.ParticipantesActivos * 100 / metricas.TotalParticipantes : 0,
                    ColorAmarillo));

                row.ConstantItem(10);

                // KPI 3: Actividades
                row.RelativeItem().Element(c => ComposeKPICard(c,
                    "📋 Actividades",
                    $"{metricas.ActividadesEjecutadas}/{metricas.ActividadesPlanificadas}",
                    $"Cumplimiento: {metricas.PorcentajeCumplimiento:F1}%",
                    (double)metricas.PorcentajeCumplimiento,
                    ColorDorado));

                row.ConstantItem(10);

                // KPI 4: Costo por Participante
                row.RelativeItem().Element(c => ComposeKPICard(c,
                    "📊 Costo/Participante",
                    metricas.CostoPorParticipante.ToString("C2"),
                    "Eficiencia financiera",
                    0,
                    ColorPrimario,
                    showProgress: false));
            });
        });
    }

    private void ComposeKPICard(IContainer container, string titulo, string valor, string subtitulo, double progreso, string color, bool showProgress = true)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Background(ColorFondo).Padding(10).Column(col =>
        {
            col.Item().Text(titulo).FontSize(8).FontColor(ColorSecundario);
            col.Item().PaddingTop(5).Text(valor).FontSize(16).Bold().FontColor(color);
            col.Item().PaddingTop(3).Text(subtitulo).FontSize(8).FontColor(ColorSecundario);
            
            if (showProgress && progreso > 0)
            {
                col.Item().PaddingTop(5).Height(6).Row(row =>
                {
                    row.RelativeItem((float)Math.Min(progreso, 100)).Background(color);
                    row.RelativeItem((float)Math.Max(100 - progreso, 0)).Background(Colors.Grey.Lighten3);
                });
            }
        });
    }

    private void ComposeGraficosVisuales(IContainer container, MetricasPOAViewModel metricas)
    {
        container.Row(row =>
        {
            // Gráfico de Cumplimiento
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().Text("📈 Cumplimiento de Actividades").FontSize(10).Bold().FontColor(ColorPrimario);
                col.Item().PaddingTop(10).Element(c => ComposeBarraProgreso(c, 
                    "Ejecutadas", metricas.ActividadesEjecutadas, metricas.ActividadesPlanificadas, ColorVerde));
                col.Item().PaddingTop(5).Element(c => ComposeBarraProgreso(c, 
                    "Pendientes", metricas.ActividadesPlanificadas - metricas.ActividadesEjecutadas, metricas.ActividadesPlanificadas, ColorAmarillo));
            });

            row.ConstantItem(10);

            // Balance Financiero
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().Text("💵 Balance Financiero").FontSize(10).Bold().FontColor(ColorPrimario);
                col.Item().PaddingTop(10).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Ingresos").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text(metricas.TotalIngresos.ToString("C0")).FontSize(14).Bold().FontColor(ColorVerde);
                    });
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Egresos").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text(metricas.TotalEgresos.ToString("C0")).FontSize(14).Bold().FontColor(ColorAmarillo);
                    });
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Balance").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text(metricas.Balance.ToString("C0")).FontSize(14).Bold()
                            .FontColor(metricas.Balance >= 0 ? ColorVerde : "#E57373");
                    });
                });
            });
        });
    }

    private void ComposeBarraProgreso(IContainer container, string etiqueta, int valor, int total, string color)
    {
        var porcentaje = total > 0 ? (float)valor * 100 / total : 0;
        container.Row(row =>
        {
            row.ConstantItem(80).Text(etiqueta).FontSize(9);
            row.RelativeItem().Height(12).Row(bar =>
            {
                bar.RelativeItem(porcentaje).Background(color);
                bar.RelativeItem(100 - porcentaje).Background(Colors.Grey.Lighten3);
            });
            row.ConstantItem(50).AlignRight().Text($"{valor} ({porcentaje:F0}%)").FontSize(8);
        });
    }

    private void ComposeDetalleOperativo(IContainer container, MetricasPOAViewModel metricas)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
        {
            col.Item().Background(ColorPrimario).Padding(8)
                .Text("📋 DETALLE OPERATIVO").FontSize(10).Bold().FontColor(ColorTextoClaro);

            col.Item().Padding(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ColorFondo).Padding(5).Text("Concepto").Bold().FontSize(9);
                    header.Cell().Background(ColorFondo).Padding(5).AlignRight().Text("Planificado").Bold().FontSize(9);
                    header.Cell().Background(ColorFondo).Padding(5).AlignRight().Text("Ejecutado").Bold().FontSize(9);
                });

                table.Cell().Padding(5).Text("Actividades").FontSize(9);
                table.Cell().Padding(5).AlignRight().Text(metricas.ActividadesPlanificadas.ToString()).FontSize(9);
                table.Cell().Padding(5).AlignRight().Text(metricas.ActividadesEjecutadas.ToString()).FontSize(9);

                table.Cell().Padding(5).Text("Presupuesto").FontSize(9);
                table.Cell().Padding(5).AlignRight().Text(metricas.PresupuestoTotal.ToString("C0")).FontSize(9);
                table.Cell().Padding(5).AlignRight().Text(metricas.PresupuestoEjecutado.ToString("C0")).FontSize(9);

                table.Cell().Padding(5).Text("Participantes").FontSize(9);
                table.Cell().Padding(5).AlignRight().Text(metricas.TotalParticipantes.ToString()).FontSize(9);
                table.Cell().Padding(5).AlignRight().Text(metricas.ParticipantesActivos.ToString()).FontSize(9);

                if (metricas.CapacitacionesRealizadas > 0)
                {
                    table.Cell().Padding(5).Text("Capacitaciones").FontSize(9);
                    table.Cell().Padding(5).AlignRight().Text("-").FontSize(9);
                    table.Cell().Padding(5).AlignRight().Text(metricas.CapacitacionesRealizadas.ToString()).FontSize(9);
                }
            });
        });
    }

    private void ComposeImpactoSocial(IContainer container, MetricasPOAViewModel metricas)
    {
        if (metricas.FamiliasImpactadas == 0 && metricas.ComunidadesAlcanzadas == 0 && 
            metricas.ObjetivosTotales == 0 && metricas.PersonasCapacitadas == 0)
            return;

        container.Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
        {
            col.Item().Background(ColorVerde).Padding(8)
                .Text("🌍 IMPACTO SOCIAL").FontSize(10).Bold().FontColor(ColorPrimario);

            col.Item().Padding(10).Row(row =>
            {
                if (metricas.ObjetivosTotales > 0)
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Objetivos").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text($"{metricas.ObjetivosCumplidos}/{metricas.ObjetivosTotales}").FontSize(14).Bold();
                        c.Item().Text($"{metricas.PorcentajeObjetivos:F0}% cumplidos").FontSize(8).FontColor(ColorVerde);
                    });
                }

                if (metricas.FamiliasImpactadas > 0)
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Familias").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text(metricas.FamiliasImpactadas.ToString("N0")).FontSize(14).Bold();
                        c.Item().Text("impactadas").FontSize(8).FontColor(ColorSecundario);
                    });
                }

                if (metricas.ComunidadesAlcanzadas > 0)
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Comunidades").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text(metricas.ComunidadesAlcanzadas.ToString("N0")).FontSize(14).Bold();
                        c.Item().Text("alcanzadas").FontSize(8).FontColor(ColorSecundario);
                    });
                }

                if (metricas.PersonasCapacitadas > 0)
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Capacitados").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text(metricas.PersonasCapacitadas.ToString("N0")).FontSize(14).Bold();
                        c.Item().Text("personas").FontSize(8).FontColor(ColorSecundario);
                    });
                }

                if (metricas.CertificadosEmitidos > 0)
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Certificados").FontSize(9).FontColor(ColorSecundario);
                        c.Item().Text(metricas.CertificadosEmitidos.ToString("N0")).FontSize(14).Bold();
                        c.Item().Text("emitidos").FontSize(8).FontColor(ColorSecundario);
                    });
                }
            });
        });
    }

    private void ComposeRecursosYAlianzas(IContainer container, MetricasPOAViewModel metricas)
    {
        if (metricas.TotalEquipo == 0 && metricas.AlianzasActivas == 0)
            return;

        container.Row(row =>
        {
            // Recursos Humanos
            if (metricas.TotalEquipo > 0)
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
                {
                    col.Item().Background(ColorAmarillo).Padding(8)
                        .Text("👷 RECURSOS HUMANOS").FontSize(10).Bold().FontColor(ColorPrimario);

                    col.Item().Padding(10).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Facilitadores").FontSize(9).FontColor(ColorSecundario);
                            c.Item().Text(metricas.TotalFacilitadores.ToString()).FontSize(14).Bold();
                        });
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Voluntarios").FontSize(9).FontColor(ColorSecundario);
                            c.Item().Text(metricas.TotalVoluntarios.ToString()).FontSize(14).Bold();
                        });
                        if (metricas.HorasVoluntariado > 0)
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Horas Vol.").FontSize(9).FontColor(ColorSecundario);
                                c.Item().Text(metricas.HorasVoluntariado.ToString("N0")).FontSize(14).Bold();
                            });
                        }
                    });

                    col.Item().PaddingHorizontal(10).PaddingBottom(10)
                        .Text($"Ratio participantes/equipo: {(metricas.TotalEquipo > 0 ? metricas.TotalParticipantes / metricas.TotalEquipo : 0)}:1")
                        .FontSize(8).Italic().FontColor(ColorSecundario);
                });
            }

            if (metricas.TotalEquipo > 0 && metricas.AlianzasActivas > 0)
                row.ConstantItem(10);

            // Alianzas
            if (metricas.AlianzasActivas > 0)
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
                {
                    col.Item().Background(ColorDorado).Padding(8)
                        .Text("🤝 ALIANZAS").FontSize(10).Bold().FontColor(ColorPrimario);

                    col.Item().Padding(10).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Alianzas Activas").FontSize(9).FontColor(ColorSecundario);
                            c.Item().Text(metricas.AlianzasActivas.ToString()).FontSize(14).Bold();
                        });
                        if (metricas.AportesEnEspecie > 0)
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Aportes en Especie").FontSize(9).FontColor(ColorSecundario);
                                c.Item().Text(metricas.AportesEnEspecie.ToString("C0")).FontSize(14).Bold();
                            });
                        }
                    });
                });
            }
        });
    }

    private void ComposeAnalisisCualitativo(IContainer container, MetricasPOAViewModel metricas)
    {
        if (string.IsNullOrWhiteSpace(metricas.LogrosPrincipales) &&
            string.IsNullOrWhiteSpace(metricas.RetosEnfrentados) &&
            string.IsNullOrWhiteSpace(metricas.LeccionesAprendidas) &&
            string.IsNullOrWhiteSpace(metricas.ProximosPasos))
            return;

        container.Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
        {
            col.Item().Background(ColorSecundario).Padding(8)
                .Text("📝 ANÁLISIS CUALITATIVO").FontSize(10).Bold().FontColor(ColorTextoClaro);

            col.Item().Padding(10).Column(content =>
            {
                if (!string.IsNullOrWhiteSpace(metricas.LogrosPrincipales))
                {
                    content.Item().Column(c =>
                    {
                        c.Item().Text("✅ Logros Principales").FontSize(9).Bold().FontColor(ColorVerde);
                        c.Item().PaddingTop(3).Text(metricas.LogrosPrincipales).FontSize(9);
                    });
                    content.Item().PaddingVertical(5);
                }

                if (!string.IsNullOrWhiteSpace(metricas.RetosEnfrentados))
                {
                    content.Item().Column(c =>
                    {
                        c.Item().Text("⚠️ Retos Enfrentados").FontSize(9).Bold().FontColor(ColorAmarillo);
                        c.Item().PaddingTop(3).Text(metricas.RetosEnfrentados).FontSize(9);
                    });
                    content.Item().PaddingVertical(5);
                }

                if (!string.IsNullOrWhiteSpace(metricas.LeccionesAprendidas))
                {
                    content.Item().Column(c =>
                    {
                        c.Item().Text("💡 Lecciones Aprendidas").FontSize(9).Bold().FontColor(ColorDorado);
                        c.Item().PaddingTop(3).Text(metricas.LeccionesAprendidas).FontSize(9);
                    });
                    content.Item().PaddingVertical(5);
                }

                if (!string.IsNullOrWhiteSpace(metricas.ProximosPasos))
                {
                    content.Item().Column(c =>
                    {
                        c.Item().Text("🎯 Próximos Pasos").FontSize(9).Bold().FontColor(ColorPrimario);
                        c.Item().PaddingTop(3).Text(metricas.ProximosPasos).FontSize(9);
                    });
                }
            });
        });
    }

    private void ComposeNotas(IContainer container, string notas)
    {
        container.Background(ColorFondo).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
        {
            col.Item().Text("📌 Notas Adicionales").FontSize(9).Bold().FontColor(ColorPrimario);
            col.Item().PaddingTop(5).Text(notas).FontSize(9).FontColor(ColorSecundario);
        });
    }

    #endregion

    #region Resumen Ejecutivo

    private void ComposeResumenEjecutivo(IContainer container, POAInstancia instancia, MetricasPOAViewModel metricas)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            column.Item().Text("RESUMEN EJECUTIVO").FontSize(14).Bold().FontColor(ColorPrimario);

            // KPIs principales
            column.Item().Element(c => ComposeDashboardKPIs(c, metricas));

            // Gráficos
            column.Item().Element(c => ComposeGraficosVisuales(c, metricas));

            // Conclusiones
            if (!string.IsNullOrWhiteSpace(metricas.LogrosPrincipales) || !string.IsNullOrWhiteSpace(metricas.ProximosPasos))
            {
                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(metricas.LogrosPrincipales))
                    {
                        col.Item().Text("Logros Destacados:").FontSize(10).Bold().FontColor(ColorVerde);
                        col.Item().PaddingTop(5).Text(metricas.LogrosPrincipales).FontSize(9);
                    }
                    if (!string.IsNullOrWhiteSpace(metricas.ProximosPasos))
                    {
                        col.Item().PaddingTop(10).Text("Próximos Pasos:").FontSize(10).Bold().FontColor(ColorPrimario);
                        col.Item().PaddingTop(5).Text(metricas.ProximosPasos).FontSize(9);
                    }
                });
            }
        });
    }

    #endregion

    #region Footer

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Height(2).Background(ColorVerde);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("ONG Juventud Sin Límites").FontSize(8).FontColor(ColorSecundario);
                    text.Span(" | ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" de ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        });
    }

    #endregion

    #region Métodos Auxiliares

    private string ObtenerRutaLogo(string clavePrograma)
    {
        var logoFileName = clavePrograma switch
        {
            "EDV" => "escueladevida.png",
            "ACADEMIA" => "Academias.png",
            "JUVENTUD_SEGURA" => "JuventudSegura.png",
            "BERNABE" => "bernabe.png",
            _ => "bernabe.png"
        };

        return Path.Combine(_environment.WebRootPath, "images", logoFileName);
    }

    private string FormatearPeriodo(POAInstancia instancia)
    {
        if (instancia.PeriodoMes.HasValue)
        {
            var nombreMes = new DateTime(instancia.PeriodoAnio, instancia.PeriodoMes.Value, 1)
                .ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));
            return char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);
        }
        return instancia.PeriodoAnio.ToString();
    }

    private string ObtenerNombreEstado(EstadoInstancia estado)
    {
        return estado switch
        {
            EstadoInstancia.Borrador => "Borrador",
            EstadoInstancia.EnRevision => "En Revisión",
            EstadoInstancia.Aprobado => "Aprobado",
            _ => "Desconocido"
        };
    }

    private string ObtenerColorEstadoPdf(EstadoInstancia estado)
    {
        return estado switch
        {
            EstadoInstancia.Borrador => ColorAmarillo,
            EstadoInstancia.EnRevision => "#42A5F5",
            EstadoInstancia.Aprobado => ColorVerde,
            _ => Colors.Grey.Medium
        };
    }

    #endregion
}
