using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

public class MotorService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MotorService> _logger;

    public MotorService(ApplicationDbContext dbContext, ILogger<MotorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Elimina alertas duplicadas basándose en ReglaId, ProgramaId, ActividadId, ParticipanteId y Mensaje
    /// Mantiene solo la alerta más reciente de cada grupo
    /// </summary>
    public async Task<int> LimpiarAlertasDuplicadasAsync(int? programaId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("🧹 Iniciando limpieza de alertas duplicadas. ProgramaId: {ProgramaId}", programaId);

        try
        {
            // Query base de alertas activas
            var query = _dbContext.Alertas.Where(a => !a.IsDeleted);

            // Filtrar por programa si se especifica
            if (programaId.HasValue)
            {
                query = query.Where(a => a.ProgramaId == programaId.Value);
            }

            // Obtener todas las alertas
            var alertas = await query.ToListAsync(ct);

            _logger.LogInformation("📊 Total de alertas a evaluar: {Count}", alertas.Count);

            // Agrupar por criterios de duplicación
            var grupos = alertas.GroupBy(a => new
            {
                a.ReglaId,
                a.ProgramaId,
                a.ActividadId,
                a.ParticipanteId,
                a.Mensaje,
                a.Severidad,
                a.Estado
            });

            var totalDuplicados = 0;
            var alertasAEliminar = new List<Alerta>();

            foreach (var grupo in grupos)
            {
                // Si el grupo tiene más de una alerta, hay duplicados
                if (grupo.Count() > 1)
                {
                    _logger.LogInformation(
                        "🔍 Grupo duplicado encontrado: Regla={ReglaId}, Programa={ProgramaId}, Actividad={ActividadId}, Participante={ParticipanteId}, Total={Count}",
                        grupo.Key.ReglaId,
                        grupo.Key.ProgramaId,
                        grupo.Key.ActividadId,
                        grupo.Key.ParticipanteId,
                        grupo.Count()
                    );

                    // Ordenar por fecha de generación descendente y mantener solo la más reciente
                    var alertasOrdenadas = grupo.OrderByDescending(a => a.GeneradaEn).ToList();
                    var alertaMasReciente = alertasOrdenadas.First();

                    // Marcar las demás como eliminadas (soft delete)
                    for (int i = 1; i < alertasOrdenadas.Count; i++)
                    {
                        var alerta = alertasOrdenadas[i];
                        alerta.IsDeleted = true;
                        alerta.ActualizadoEn = DateTime.UtcNow;
                        alertasAEliminar.Add(alerta);
                        totalDuplicados++;
                    }

                    _logger.LogInformation(
                        "✅ Mantenida alerta ID={AlertaId} (más reciente: {Fecha}), eliminadas {Count} duplicadas",
                        alertaMasReciente.AlertaId,
                        alertaMasReciente.GeneradaEn,
                        alertasOrdenadas.Count - 1
                    );
                }
            }

            if (totalDuplicados > 0)
            {
                await _dbContext.SaveChangesAsync(ct);
                _logger.LogInformation("✅ Limpieza completada. {Total} alertas duplicadas eliminadas", totalDuplicados);
            }
            else
            {
                _logger.LogInformation("✅ No se encontraron alertas duplicadas");
            }

            return totalDuplicados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al limpiar alertas duplicadas");
            throw;
        }
    }

    /// <summary>
    /// Obtiene estadísticas del motor para el dashboard
    /// </summary>
    public async Task<EstadisticasMotor> ObtenerEstadisticasAsync(Usuario? usuario, bool esCoordinador, CancellationToken ct = default)
    {
        var query = _dbContext.Alertas.Where(a => !a.IsDeleted);

        // Filtrar por programas del coordinador si aplica
        if (esCoordinador && usuario != null)
        {
            var programasIds = usuario.UsuarioProgramas
                .Where(up => !up.IsDeleted)
                .Select(up => up.ProgramaId)
                .ToList();

            query = query.Where(a => a.ProgramaId.HasValue && programasIds.Contains(a.ProgramaId.Value));
        }

        var totalAlertas = await query.CountAsync(ct);
        var alertasAbiertas = await query.CountAsync(a => a.Estado == EstadoAlerta.Abierta, ct);
        var alertasResueltas = await query.CountAsync(a => a.Estado == EstadoAlerta.Resuelta, ct);
        var alertasCriticas = await query.CountAsync(a => a.Severidad == Severidad.Critica, ct);

        // Datos por severidad
        var info = await query.CountAsync(a => a.Severidad == Severidad.Info, ct);
        var alta = await query.CountAsync(a => a.Severidad == Severidad.Alta, ct);
        var critica = await query.CountAsync(a => a.Severidad == Severidad.Critica, ct);

        // Datos por estado
        var descartadas = await query.CountAsync(a => a.Estado == EstadoAlerta.Descartada, ct);

        return new EstadisticasMotor
        {
            TotalAlertas = totalAlertas,
            AlertasAbiertas = alertasAbiertas,
            AlertasResueltas = alertasResueltas,
            AlertasCriticas = alertasCriticas,
            SeveridadInfo = info,
            SeveridadAlta = alta,
            SeveridadCritica = critica,
            EstadoAbiertas = alertasAbiertas,
            EstadoResueltas = alertasResueltas,
            EstadoDescartadas = descartadas
        };
    }
}

public class EstadisticasMotor
{
    public int TotalAlertas { get; set; }
    public int AlertasAbiertas { get; set; }
    public int AlertasResueltas { get; set; }
    public int AlertasCriticas { get; set; }
    
    // Datos para gráficas
    public int SeveridadInfo { get; set; }
    public int SeveridadAlta { get; set; }
    public int SeveridadCritica { get; set; }
    
    public int EstadoAbiertas { get; set; }
    public int EstadoResueltas { get; set; }
    public int EstadoDescartadas { get; set; }
}
