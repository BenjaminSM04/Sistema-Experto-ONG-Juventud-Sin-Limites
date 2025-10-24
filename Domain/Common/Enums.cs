namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

// Estados generales
public enum EstadoGeneral : byte
{
    Inactivo = 0,
    Activo = 1
}

// Actividad
public enum TipoActividad : byte
{
    Taller = 1,
    Capacitacion = 2,
  Evento = 3,
    Reunion = 4,
    Otro = 5
}

public enum EstadoActividad : byte
{
    Planificada = 1,
    Realizada = 2,
    Cerrada = 3,
    Cancelada = 4
}

// ActividadParticipante
public enum RolParticipante : byte
{
    Asistente = 1,
    Facilitador = 2,
    Coordinador = 3
}

public enum EstadoInscripcion : byte
{
    Inscrito = 1,
    Confirmado = 2,
    Retirado = 3
}

// Asistencia
public enum EstadoAsistencia : byte
{
    Presente = 1,
    Ausente = 2,
    Tarde = 3,
    Justificado = 4
}

// Evidencia
public enum TipoEvidencia : byte
{
    Foto = 1,
    Acta = 2,
 Lista = 3,
    Video = 4,
    Otro = 5
}

// POA
public enum EstadoPlantilla : byte
{
    Borrador = 1,
    Activa = 2,
    Deprecada = 3
}

public enum TipoDato : byte
{
    Texto = 1,
    Entero = 2,
 Decimal = 3,
    Fecha = 4,
    Bool = 5,
    Lista = 6
}

public enum AlcancePOA : byte
{
    Programa = 1,
    Actividad = 2,
    Participante = 3
}

public enum TipoValidacion : byte
{
    Rango = 1,
    Regex = 2,
    Longitud = 3,
    Dependencia = 4
}

public enum EstadoInstancia : byte
{
    Borrador = 1,
 EnRevision = 2,
    Aprobado = 3
}

// Motor de inferencia
public enum Severidad : byte
{
    Info = 1,
    Alta = 2,
    Critica = 3
}

public enum ObjetivoRegla : byte
{
    Participante = 1,
    Actividad = 2,
    Programa = 3
}

public enum TipoParametro : byte
{
    Entero = 1,
    Decimal = 2,
    Bool = 3,
    Texto = 4
}

public enum EstadoAlerta : byte
{
 Abierta = 1,
    Resuelta = 2,
    Descartada = 3
}

public enum BandaRiesgo : byte
{
    Bajo = 1,
    Medio = 2,
    Alto = 3
}

public enum AmbitoEjecucion : byte
{
    EDV = 1,
    Academia = 2,
    Todos = 3
}

public enum AmbitoDiccionario : byte
{
    Global = 0,
    EDV = 1,
    Academia = 2
}
