using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using System.Text;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

/// <summary>
/// Servicio para generar reportes de programas
/// </summary>
public class ReportesProgramaService
{
    private readonly ApplicationDbContext _context;

    public ReportesProgramaService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera un reporte HTML detallado del programa para un año específico
    /// </summary>
    public async Task<string> GenerarReporteHtmlAsync(int programaId, int anio)
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

        var html = new StringBuilder();
        
// Estilos CSS
        html.AppendLine(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Reporte " + programa.Nombre + @" - " + anio + @"</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 40px;
            color: #333;
    }
      .header {
            background: linear-gradient(90deg, #4D3935 0%, #6D534F 100%);
            color: #FEFEFD;
        padding: 30px;
 border-radius: 8px;
            margin-bottom: 30px;
  }
     .header h1 {
    margin: 0 0 10px 0;
        }
    .header .subtitle {
       opacity: 0.9;
 font-size: 14px;
        }
        .section {
            margin-bottom: 30px;
        }
        .section-title {
     background-color: #F7C484;
   padding: 15px;
        border-radius: 8px 8px 0 0;
            font-weight: 600;
        font-size: 18px;
          color: #4D3935;
      }
        .section-content {
         background-color: #FEFEFD;
            padding: 20px;
border: 1px solid #e0e0e0;
            border-top: none;
    border-radius: 0 0 8px 8px;
        }
        table {
        width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }
        th {
   background-color: #4D3935;
        color: #FEFEFD;
   padding: 12px;
    text-align: left;
         font-weight: 600;
        }
        td {
            padding: 10px 12px;
    border-bottom: 1px solid #e0e0e0;
 }
    tr:hover {
     background-color: #f5f5f5;
        }
        .kpi-grid {
  display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 20px;
     margin: 20px 0;
        }
      .kpi-card {
         background: linear-gradient(135deg, #FEFEFD 0%, #F7F7F7 100%);
            border-left: 4px solid #9FD996;
            padding: 20px;
            border-radius: 8px;
        }
        .kpi-value {
  font-size: 32px;
     font-weight: 700;
      color: #4D3935;
    margin: 10px 0;
        }
        .kpi-label {
            font-size: 12px;
        color: #6D534F;
   text-transform: uppercase;
   letter-spacing: 0.5px;
   }
      .badge {
 display: inline-block;
            padding: 4px 12px;
            border-radius: 4px;
            font-size: 12px;
         font-weight: 600;
        }
        .badge-success {
            background-color: #9FD996;
    color: #4D3935;
        }
     .badge-warning {
     background-color: #F3C95A;
 color: #4D3935;
      }
   .badge-error {
   background-color: #F7C484;
        color: #4D3935;
        }
 .badge-info {
  background-color: #87CEEB;
        color: #4D3935;
        }
        .footer {
  margin-top: 50px;
       padding-top: 20px;
            border-top: 2px solid #e0e0e0;
            text-align: center;
            color: #6D534F;
  font-size: 12px;
    }
        @media print {
       body { margin: 20px; }
            .section { page-break-inside: avoid; }
        }
    </style>
</head>
<body>
");

        // Header
        html.AppendLine($@"
<div class='header'>
        <h1>{programa.Nombre}</h1>
        <div class='subtitle'>Clave: {programa.Clave} | Año: {anio}</div>
  <div class='subtitle'>{programa.Descripcion}</div>
        <div class='subtitle' style='margin-top: 10px;'>
            Estado: <span class='badge badge-{(programa.Estado == EstadoGeneral.Activo ? "success" : "error")}'>{programa.Estado}</span>
            Inferencia: <span class='badge badge-{(programa.InferenciaActiva ? "success" : "warning")}'>{(programa.InferenciaActiva ? "Activa" : "Inactiva")}</span>
    </div>
    </div>
");

        // KPIs Anuales
        var totalPlanificadas = metricas.Sum(m => m.ActividadesPlanificadas);
        var totalEjecutadas = metricas.Sum(m => m.ActividadesEjecutadas);
        var cumplimientoPromedio = metricas.Any() ? metricas.Average(m => (double)m.PorcCumplimiento) : 0;
 var asistenciaPromedio = metricas.Any() ? metricas.Average(m => (double)m.PorcAsistenciaProm) : 0;

        html.AppendLine($@"
    <div class='section'>
        <div class='section-title'>KPIs Anuales {anio}</div>
  <div class='section-content'>
            <div class='kpi-grid'>
      <div class='kpi-card'>
         <div class='kpi-label'>Actividades Planificadas</div>
              <div class='kpi-value'>{totalPlanificadas}</div>
        </div>
  <div class='kpi-card'>
            <div class='kpi-label'>Actividades Ejecutadas</div>
               <div class='kpi-value'>{totalEjecutadas}</div>
</div>
  <div class='kpi-card'>
              <div class='kpi-label'>% Cumplimiento Promedio</div>
          <div class='kpi-value'>{cumplimientoPromedio:F1}%</div>
          </div>
       <div class='kpi-card'>
          <div class='kpi-label'>% Asistencia Promedio</div>
     <div class='kpi-value'>{asistenciaPromedio:F1}%</div>
                </div>
            </div>
 </div>
    </div>
");

        // Actividades
        html.AppendLine($@"
    <div class='section'>
        <div class='section-title'>Actividades Realizadas ({actividades.Count})</div>
        <div class='section-content'>
            <table>
         <thead>
         <tr>
  <th>Título</th>
   <th>Fecha</th>
     <th>Tipo</th>
 <th>Estado</th>
         <th>Lugar</th>
        </tr>
          </thead>
   <tbody>
");

    foreach (var actividad in actividades)
     {
       var badgeClass = actividad.Estado switch
            {
        EstadoActividad.Realizada => "badge-success",
     EstadoActividad.Planificada => "badge-warning",
            EstadoActividad.Cancelada => "badge-error",
            _ => "badge-info"
            };

   html.AppendLine($@"
        <tr>
  <td><strong>{actividad.Titulo}</strong></td>
                   <td>{actividad.FechaInicio:dd/MM/yyyy HH:mm}</td>
       <td>{actividad.Tipo}</td>
     <td><span class='badge {badgeClass}'>{actividad.Estado}</span></td>
        <td>{actividad.Lugar ?? "N/A"}</td>
          </tr>
");
      }

        html.AppendLine(@"
</tbody>
            </table>
        </div>
    </div>
");

  // Métricas Mensuales
     html.AppendLine($@"
    <div class='section'>
      <div class='section-title'>Métricas Mensuales</div>
    <div class='section-content'>
    <table>
      <thead>
        <tr>
      <th>Mes</th>
       <th>Planificadas</th>
                <th>Ejecutadas</th>
       <th>% Cumplimiento</th>
      <th>% Asistencia</th>
      <th>Retraso (días)</th>
          </tr>
  </thead>
          <tbody>
");

   foreach (var metrica in metricas)
        {
  html.AppendLine($@"
          <tr>
       <td><strong>{metrica.AnioMes}</strong></td>
     <td>{metrica.ActividadesPlanificadas}</td>
       <td>{metrica.ActividadesEjecutadas}</td>
       <td>{metrica.PorcCumplimiento:F1}%</td>
         <td>{metrica.PorcAsistenciaProm:F1}%</td>
   <td>{metrica.RetrasoPromedioDias:F1}</td>
  </tr>
");
        }

      html.AppendLine(@"
                </tbody>
            </table>
        </div>
    </div>
");

   // Alertas
        html.AppendLine($@"
    <div class='section'>
        <div class='section-title'>Alertas Generadas ({alertas.Count})</div>
        <div class='section-content'>
     <table>
              <thead>
             <tr>
         <th>Fecha</th>
            <th>Severidad</th>
   <th>Regla</th>
            <th>Mensaje</th>
    <th>Estado</th>
 </tr>
    </thead>
        <tbody>
");

        foreach (var alerta in alertas.Take(50)) // Primeras 50
        {
            var severidadClass = alerta.Severidad switch
         {
    Domain.Common.Severidad.Critica => "badge-error",
      Domain.Common.Severidad.Alta => "badge-warning",
        _ => "badge-info"
      };

var estadoClass = alerta.Estado switch
            {
                Domain.Common.EstadoAlerta.Abierta => "badge-error",
          Domain.Common.EstadoAlerta.Resuelta => "badge-success",
       _ => "badge-warning"
            };

  html.AppendLine($@"
        <tr>
         <td>{alerta.GeneradaEn:dd/MM/yyyy}</td>
              <td><span class='badge {severidadClass}'>{alerta.Severidad}</span></td>
 <td>{alerta.Regla?.Nombre ?? "N/A"}</td>
          <td>{alerta.Mensaje}</td>
    <td><span class='badge {estadoClass}'>{alerta.Estado}</span></td>
 </tr>
");
        }

   html.AppendLine(@"
        </tbody>
    </table>
     </div>
    </div>
");

      // Footer
        html.AppendLine($@"
    <div class='footer'>
        <p>Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}</p>
        <p>ONG Juventud Sin Límites - Sistema Experto de Gestión</p>
    </div>
</body>
</html>
");

   return html.ToString();
    }
}
