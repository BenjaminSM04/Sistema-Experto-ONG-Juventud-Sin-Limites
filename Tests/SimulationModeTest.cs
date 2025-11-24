using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Tests;

public class SimulationModeTest
{
    [Fact]
    public async Task EjecutarAsync_DryRunTrue_NoGuardaAlertas()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_DryRun_" + Guid.NewGuid())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        // Seed Data
        var regla = new Regla
        {
            ReglaId = 1,
            Clave = "INASISTENCIA_CONSECUTIVA",
            Objetivo = ObjetivoRegla.Participante,
            Activa = true,
            Severidad = Severidad.Alta
        };
        context.Reglas.Add(regla);

        context.ReglaParametros.Add(new ReglaParametro
        {
            ReglaId = 1,
            Nombre = "UMBRAL_AUSENCIAS",
            Tipo = TipoParametro.Entero,
            Valor = "3"
        });

        context.Actividades.Add(new Domain.Operacion.Actividad
        {
            ActividadId = 1,
            Titulo = "Actividad Test",
            ProgramaId = 1
        });

        context.ActividadParticipantes.Add(new Domain.Operacion.ActividadParticipante
        {
            ActividadId = 1,
            ParticipanteId = 100,
            Estado = EstadoInscripcion.Inscrito
        });

        await context.SaveChangesAsync();

        var fakeFeatures = new FakeFeatureProvider();
        // Configurar fake para devolver 5 inasistencias (mayor que umbral 3)
        fakeFeatures.ConsecutivasAusenciasResult = 5;

        var motor = new MotorInferencia(context, fakeFeatures);

        // Act - Dry Run TRUE
        var resumenDry = await motor.EjecutarAsync(DateOnly.FromDateTime(DateTime.Now), null, dryRun: true, CancellationToken.None);

        // Assert - Dry Run TRUE
        Assert.Equal(0, await context.Alertas.CountAsync()); // No debe haber alertas en BD
        // Nota: En la implementación actual, el resumen cuenta las alertas "generadas" (aunque sean simuladas) en la propiedad AlertasGeneradas
        // Si cambiamos la lógica para que AlertasGeneradas sea 0 en dryRun, ajustamos aquí. 
        // Pero según mi código: ejec.Exitos = alertas; y retorna (reglas, alertas, errores).
        // Así que AlertasGeneradas debería ser > 0 si encontró problemas, aunque no los guarde.
        Assert.True(resumenDry.AlertasGeneradas > 0, "Debería reportar alertas simuladas");

        // Act - Dry Run FALSE
        var resumenReal = await motor.EjecutarAsync(DateOnly.FromDateTime(DateTime.Now), null, dryRun: false, CancellationToken.None);

        // Assert - Dry Run FALSE
        Assert.NotEqual(0, await context.Alertas.CountAsync()); // Debe haber alertas en BD
        Assert.True(resumenReal.AlertasGeneradas > 0);
    }
}

public class FakeFeatureProvider : IFeatureProvider
{
    public int ConsecutivasAusenciasResult { get; set; } = 0;

    public Task<int> ConsecutivasAusenciasAsync(int participanteId, int actividadId, DateOnly hasta, CancellationToken ct)
    {
        return Task.FromResult(ConsecutivasAusenciasResult);
    }

    public Task<(int plan, int ejec)> PlanVsEjecAsync(int programaId, string anioMes, CancellationToken ct)
    {
        return Task.FromResult((0, 0));
    }

    public Task<decimal?> PoaDecimalAsync(int instanciaId, string campoClave, int? programaId, int? actividadId, int? participanteId, CancellationToken ct)
    {
        return Task.FromResult<decimal?>(null);
    }

    public Task<double> PorcAsistenciaParticipanteAsync(int participanteId, int programaId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        return Task.FromResult(100.0);
    }
}
