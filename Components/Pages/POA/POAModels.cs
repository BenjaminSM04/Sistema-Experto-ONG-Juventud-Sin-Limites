namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Pages.POA;

public class POAViewModel
{
    public int InstanciaId { get; set; }
    public int ProgramaId { get; set; }
    public string ProgramaNombre { get; set; } = string.Empty;
    public string ProgramaClave { get; set; } = string.Empty;
    public int PlantillaId { get; set; }
    public short PeriodoAnio { get; set; }
    public byte? PeriodoMes { get; set; }
    public string PeriodoTexto => PeriodoMes.HasValue 
        ? $"{ObtenerNombreMes(PeriodoMes.Value)} {PeriodoAnio}" 
        : $"{PeriodoAnio}";
    public string Estado { get; set; } = string.Empty;
    public string? Notas { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    
    private string ObtenerNombreMes(byte mes)
    {
        return mes switch
        {
            1 => "Enero",
            2 => "Febrero",
            3 => "Marzo",
            4 => "Abril",
            5 => "Mayo",
            6 => "Junio",
            7 => "Julio",
            8 => "Agosto",
            9 => "Septiembre",
            10 => "Octubre",
            11 => "Noviembre",
            12 => "Diciembre",
            _ => "N/A"
        };
    }
}

public class PlantillaPOAViewModel
{
    public int PlantillaId { get; set; }
    public int ProgramaId { get; set; }
    public int Version { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
    public List<SeccionPOAViewModel> Secciones { get; set; } = new();
}

public class SeccionPOAViewModel
{
    public int SeccionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public List<CampoPOAViewModel> Campos { get; set; } = new();
}

public class CampoPOAViewModel
{
    public int CampoId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string TipoDato { get; set; } = string.Empty;
    public bool Requerido { get; set; }
    public int Orden { get; set; }
    public string? Unidad { get; set; }
    public string Alcance { get; set; } = string.Empty;
    public List<OpcionCampoViewModel> Opciones { get; set; } = new();
    public List<ValidacionCampoViewModel> Validaciones { get; set; } = new();
    
    // Valor actual
    public string? ValorTexto { get; set; }
    public decimal? ValorNumero { get; set; }
    public DateTime? ValorFecha { get; set; }
    public bool? ValorBool { get; set; }
}

public class OpcionCampoViewModel
{
    public int OpcionId { get; set; }
    public string Valor { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
}

public class ValidacionCampoViewModel
{
    public int ValidacionId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Parametro { get; set; } = string.Empty;
}

public class POADashboardViewModel
{
    public POAViewModel POA { get; set; } = new();
    public MetricasPOAViewModel Metricas { get; set; } = new();
}

public class MetricasPOAViewModel
{
    public decimal PresupuestoTotal { get; set; }
    public int TotalParticipantes { get; set; }
    public decimal CostoPorParticipante => TotalParticipantes > 0 
        ? PresupuestoTotal / TotalParticipantes 
        : 0;
    
    public int ActividadesPlanificadas { get; set; }
    public int ActividadesEjecutadas { get; set; }
    public decimal PorcentajeCumplimiento => ActividadesPlanificadas > 0 
        ? (ActividadesEjecutadas * 100m / ActividadesPlanificadas) 
        : 0;
    
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal Balance => TotalIngresos - TotalEgresos;
    
    public List<MetricaDinamica> MetricasDinamicas { get; set; } = new();
}

public class MetricaDinamica
{
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string? Unidad { get; set; }
    public string? Icono { get; set; }
}

public class FormularioPOAModel
{
    public int InstanciaId { get; set; }
    public int ProgramaId { get; set; }
    public int PlantillaId { get; set; }
    public short PeriodoAnio { get; set; }
    public byte? PeriodoMes { get; set; }
    public string? Notas { get; set; }
    public Dictionary<int, ValorCampoModel> Valores { get; set; } = new();
}

public class ValorCampoModel
{
    public int CampoId { get; set; }
    public string? ValorTexto { get; set; }
    public decimal? ValorNumero { get; set; }
    public DateTime? ValorFecha { get; set; }
    public bool? ValorBool { get; set; }
}

public class CrearPOAModel
{
    public int ProgramaId { get; set; }
    public short PeriodoAnio { get; set; }
    public byte? PeriodoMes { get; set; }
    public string? Notas { get; set; }
    
    // Actividades
    public int ActividadesPlanificadas { get; set; }
    public int ActividadesEjecutadas { get; set; }
    
    // Presupuesto
    public decimal? PresupuestoTotal { get; set; }
    public decimal? PresupuestoEjecutado { get; set; }
    
    // Participantes
    public int? TotalParticipantes { get; set; }
    public int? ParticipantesActivos { get; set; }
    
    // Recursos Humanos
    public int? TotalFacilitadores { get; set; }
    public int? TotalVoluntarios { get; set; }
    
    // Indicadores de cumplimiento
    public decimal? PorcentajeAsistencia { get; set; }
    public decimal? PorcentajeCumplimiento { get; set; }
}

public class EditarPOAModel
{
    public int InstanciaId { get; set; }
    public int ProgramaId { get; set; }
    public string? Notas { get; set; }
    
    // Actividades
    public int ActividadesPlanificadas { get; set; }
    public int ActividadesEjecutadas { get; set; }
    
    // Presupuesto
    public decimal? PresupuestoTotal { get; set; }
    public decimal? PresupuestoEjecutado { get; set; }
    
    // Participantes
    public int? TotalParticipantes { get; set; }
    public int? ParticipantesActivos { get; set; }
    
    // Recursos Humanos
    public int? TotalFacilitadores { get; set; }
    public int? TotalVoluntarios { get; set; }
    
    // Indicadores de cumplimiento
    public decimal? PorcentajeAsistencia { get; set; }
    public decimal? PorcentajeCumplimiento { get; set; }
}
