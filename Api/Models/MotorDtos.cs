namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Api.Models;

public record MotorRunDto(int? ProgramaId, DateTime? FechaCorte, bool DryRun = false);

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

public record ReglaDto(
    int ReglaId,
    string Clave,
    string Nombre,
    string? Descripcion,
    byte Severidad,
    byte Objetivo,
    bool Activa,
    int Prioridad,
    int Version,
    List<ReglaParametroDto> Parametros,
    byte[]? RowVersion
);

public record ReglaParametroDto(
    int? ReglaParametroId,
    string Nombre,
    byte Tipo,
    string Valor,
    byte[]? RowVersion
);

public record UpsertReglaRequest(
    int? ReglaId,
    string Clave,
    string Nombre,
    string? Descripcion,
    byte Severidad,
    byte Objetivo,
    bool Activa,
    int Prioridad,
    int Version,
    List<ReglaParametroDto> Parametros,
    byte[]? RowVersion
);

public record CambiarEstadoReglaRequest(bool Activa, byte[]? RowVersion);
