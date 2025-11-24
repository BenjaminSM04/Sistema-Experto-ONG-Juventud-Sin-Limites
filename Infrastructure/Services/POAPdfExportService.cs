using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;
using System.IO;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

/// <summary>
/// Servicio para exportar POA a PDF
/// </summary>
public class POAPdfExportService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public POAPdfExportService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
        
        // Configurar licencia (Community License es gratuita)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera un PDF del POA completo
    /// </summary>
    public async Task<byte[]> GenerarPOAPdfAsync(int instanciaId)
    {
        // Obtener datos del POA
        var instancia = await _context.POAInstancias
            .Include(i => i.Programa)
            .Include(i => i.Plantilla)
                .ThenInclude(p => p!.Campos.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Seccion)
            .FirstOrDefaultAsync(i => i.InstanciaId == instanciaId && !i.IsDeleted);

        if (instancia == null)
            throw new InvalidOperationException($"No se encontró la instancia POA con ID {instanciaId}");

        // Obtener valores
        var valores = await _context.POAValores
            .Include(v => v.Campo)
            .Where(v => v.InstanciaId == instanciaId && !v.IsDeleted)
            .ToListAsync();

        // Generar documento PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Encabezado
                page.Header().Element(c => ComposeHeader(c, instancia));

                // Contenido
                page.Content().Element(c => ComposeContent(c, instancia, valores));

                // Pie de página
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                    x.Span($" | Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });
        });

        // Generar PDF en memoria
        return document.GeneratePdf();
    }

    /// <summary>
    /// Genera un PDF simplificado con solo valores clave
    /// </summary>
    public async Task<byte[]> GenerarResumenPOAPdfAsync(int instanciaId)
    {
        var instancia = await _context.POAInstancias
            .Include(i => i.Programa)
            .Include(i => i.Plantilla)
            .FirstOrDefaultAsync(i => i.InstanciaId == instanciaId && !i.IsDeleted);

        if (instancia == null)
            throw new InvalidOperationException($"No se encontró la instancia POA con ID {instanciaId}");

        // Obtener solo valores clave
        var valoresClave = await _context.POAValores
            .Include(v => v.Campo)
            .Where(v => v.InstanciaId == instanciaId 
                     && !v.IsDeleted 
                     && v.Campo!.Requerido)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, instancia));
                page.Content().Element(c => ComposeResumenContent(c, instancia, valoresClave));
                page.Footer().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
            });
        });

        return document.GeneratePdf();
    }

    #region Composición de Secciones del PDF

    private void ComposeHeader(IContainer container, POAInstancia instancia)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                // Logo del programa
                row.ConstantItem(120).Height(80).Element(c =>
                {
                    var logoPath = ObtenerRutaLogo(instancia.Programa?.Clave ?? "");
                    if (File.Exists(logoPath))
                    {
                        c.AlignCenter().AlignMiddle().Image(logoPath, ImageScaling.FitArea);
                    }
                    else
                    {
                        // Placeholder si no existe el logo
                        c.Background("#9FD996").AlignCenter().AlignMiddle().Text(instancia.Programa?.Clave ?? "")
                            .FontSize(12).SemiBold().FontColor("#4D3935");
                    }
                });

                row.RelativeItem().PaddingLeft(15).Column(col =>
                {
                    col.Item().Text("Plan Operativo Anual")
                        .FontSize(22)
                        .Bold()
                        .FontColor("#4D3935");

                    col.Item().PaddingTop(5).Text($"{instancia.Programa?.Nombre ?? "N/A"}")
                        .FontSize(16)
                        .SemiBold()
                        .FontColor("#6D534F");

                    col.Item().PaddingTop(8).Text(text =>
                    {
                        text.Span("Periodo: ").SemiBold().FontSize(11);
                        text.Span(FormatearPeriodo(instancia)).FontSize(11);
                        text.Span(" | ").FontSize(11);
                        text.Span("Estado: ").SemiBold().FontSize(11).FontColor("#4D3935");
                        text.Span(ObtenerNombreEstado(instancia.Estado))
                            .FontSize(11)
                            .Bold()
                            .FontColor(ObtenerColorEstado(instancia.Estado));
                    });
                });

                // Espacio para logo ONG (opcional)
                row.ConstantItem(100).Height(80).Element(c =>
                {
                    // Texto simple de la ONG
                    c.AlignCenter().AlignMiddle().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ONG")
                            .FontSize(14).Bold().FontColor("#00A0B0");
                        col.Item().AlignCenter().Text("Juventud")
                            .FontSize(10).SemiBold().FontColor("#4D3935");
                        col.Item().AlignCenter().Text("Sin Límites")
                            .FontSize(10).SemiBold().FontColor("#4D3935");
                    });
                });
            });

            column.Item().PaddingTop(15).BorderBottom(3).BorderColor("#9FD996");
        });
    }

    /// <summary>
    /// Obtiene la ruta del logo según la clave del programa
    /// </summary>
    private string ObtenerRutaLogo(string clavePrograma)
    {
        var logoFileName = clavePrograma switch
        {
            "EDV" => "escueladevida.png",
            "ACADEMIA" => "Academias.png",
            "JUVENTUD_SEGURA" => "JuventudSegura.png",
            "BERNABE" => "bernabe.png",
            _ => "bernabe.png" // Default a uno de los logos disponibles
        };

        return Path.Combine(_environment.WebRootPath, "images", logoFileName);
    }

    private void ComposeContent(IContainer container, POAInstancia instancia, List<POAValor> valores)
    {
        container.Column(column =>
        {
            // Información General
            column.Item().Element(c => ComposeInformacionGeneral(c, instancia));

            // Agrupar campos por sección
            var campos = instancia.Plantilla?.Campos?
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Seccion?.Orden ?? 0)
                .ThenBy(c => c.Orden)
                .ToList() ?? new List<POACampo>();

            var seccionesAgrupadas = campos
                .GroupBy(c => c.SeccionId)
                .OrderBy(g => g.FirstOrDefault()?.Seccion?.Orden ?? 0);

            foreach (var grupo in seccionesAgrupadas)
            {
                var nombreSeccion = grupo.FirstOrDefault()?.Seccion?.Nombre ?? "Sin Sección";
                
                column.Item().PaddingTop(15).Text(nombreSeccion)
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#4D3935");

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor("#E0E0E0");

                column.Item().PaddingTop(10).Element(c => ComposeCampos(c, grupo.ToList(), valores));
            }

            // Si no hay secciones, mostrar todos los campos
            if (!seccionesAgrupadas.Any())
            {
                column.Item().Element(c => ComposeCampos(c, campos, valores));
            }
        });
    }

    private void ComposeInformacionGeneral(IContainer container, POAInstancia instancia)
    {
        container.Background("#F7F7F7").Padding(15).Column(column =>
        {
            column.Item().Text("Información General")
                .FontSize(14)
                .SemiBold()
                .FontColor("#4D3935");

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Element(c => ComposeInfoItem(c, "Programa", instancia.Programa?.Nombre ?? "N/A"));
                row.RelativeItem().Element(c => ComposeInfoItem(c, "Periodo", FormatearPeriodo(instancia)));
            });

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Element(c => ComposeInfoItem(c, "Estado", ObtenerNombreEstado(instancia.Estado)));
                row.RelativeItem().Element(c => ComposeInfoItem(c, "Plantilla", $"Versión {instancia.Plantilla?.Version ?? 0}"));
            });

            if (!string.IsNullOrWhiteSpace(instancia.Notas))
            {
                column.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Notas: ").SemiBold();
                    text.Span(instancia.Notas);
                });
            }
        });
    }

    private void ComposeInfoItem(IContainer container, string label, string value)
    {
        container.Column(column =>
        {
            column.Item().Text(label).FontSize(9).FontColor("#666666");
            column.Item().Text(value).FontSize(11).SemiBold();
        });
    }

    private void ComposeCampos(IContainer container, List<POACampo> campos, List<POAValor> valores)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2); // Campo
                columns.RelativeColumn(3); // Valor
            });

            // Encabezado
            table.Header(header =>
            {
                header.Cell().Background("#4D3935").Padding(8).Text("Campo")
                    .FontColor(Colors.White).SemiBold();
                header.Cell().Background("#4D3935").Padding(8).Text("Valor")
                    .FontColor(Colors.White).SemiBold();
            });

            // Filas
            foreach (var campo in campos)
            {
                var valor = valores.FirstOrDefault(v => v.CampoId == campo.CampoId);

                table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(8).Column(column =>
                {
                    column.Item().Text(campo.Etiqueta).SemiBold();
                    if (campo.Requerido)
                    {
                        column.Item().Text("(Requerido)").FontSize(8).Italic().FontColor(Colors.Red.Medium);
                    }
                    if (!string.IsNullOrWhiteSpace(campo.Unidad))
                    {
                        column.Item().Text($"Unidad: {campo.Unidad}").FontSize(9).FontColor("#666666");
                    }
                });

                table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(8).Text(FormatearValor(valor, campo));
            }
        });
    }

    private void ComposeResumenContent(IContainer container, POAInstancia instancia, List<POAValor> valoresClave)
    {
        container.Column(column =>
        {
            // Información General
            column.Item().Element(c => ComposeInformacionGeneral(c, instancia));

            // Título de Indicadores Clave
            column.Item().PaddingTop(20).Text("Indicadores Clave")
                .FontSize(14)
                .SemiBold()
                .FontColor("#4D3935");

            // Tabla de valores clave
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                foreach (var valor in valoresClave.OrderBy(v => v.Campo?.Orden))
                {
                    table.Cell().Background("#F7F7F7").Padding(10).Text(valor.Campo?.Etiqueta ?? "N/A")
                        .SemiBold();
                    table.Cell().BorderLeft(2).BorderColor("#9FD996").Padding(10)
                        .Text(FormatearValor(valor, valor.Campo!))
                        .FontSize(14)
                        .SemiBold()
                        .FontColor("#4D3935");
                }
            });
        });
    }

    #endregion

    #region Métodos Auxiliares

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

    private string ObtenerColorEstado(EstadoInstancia estado)
    {
        return estado switch
        {
            EstadoInstancia.Borrador => "#FFA726",      // Naranja
            EstadoInstancia.EnRevision => "#42A5F5",    // Azul
            EstadoInstancia.Aprobado => "#66BB6A",      // Verde
            _ => "#757575"                              // Gris
        };
    }

    private string FormatearValor(POAValor? valor, POACampo campo)
    {
        if (valor == null)
            return "—";

        return campo.TipoDato switch
        {
            TipoDato.Texto => valor.ValorTexto ?? "—",
            TipoDato.Entero => valor.ValorNumero?.ToString("N0") ?? "—",
            TipoDato.Decimal => valor.ValorDecimal?.ToString("N2") ?? "—",
            TipoDato.Fecha => valor.ValorFecha?.ToString("dd/MM/yyyy") ?? "—",
            TipoDato.Bool => valor.ValorBool.HasValue ? (valor.ValorBool.Value ? "Sí" : "No") : "—",
            TipoDato.Lista => valor.ValorTexto ?? "—",
            _ => "—"
        };
    }

    #endregion
}
