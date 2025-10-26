namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Api.Models;

public record MotorRunDto(DateTime? FechaCorte, int? ProgramaId);


public record AlertaCambioEstadoDto(byte NuevoEstado, string? Comentario, byte[]? RowVersion);

public record MotorRunResponseDto(
    DateTime FechaCorte,
    int? ProgramaId,
    ResumenEjecucion Resumen,
    List<AlertaDto> UltimasAlertas
);


public record ResumenEjecucion(
    int ReglasEjecutadas,
    int AlertasGeneradas,
    int Errores
);

public record AlertaDto(
    int AlertaId,
    string Mensaje,
    byte Severidad,
    byte Estado,
  DateTime GeneradaEn,
    int ReglaId,
 int? ProgramaId,
    int? ActividadId,
    int? ParticipanteId,
    byte[] RowVersion
);
