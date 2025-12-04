namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Pages.Admin.Motor;

public class ProgramaInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class MotorDashboardData
{
    public int TotalAlertas { get; set; }
    public int AlertasAbiertas { get; set; }
    public int AlertasResueltas { get; set; }
    public int AlertasCriticas { get; set; }
    public int AlertasDescartadas { get; set; }
    public int ReglasActivas { get; set; }
    public double TasaResolucion { get; set; }
    public DateTime? UltimaEjecucion { get; set; }
    public int ProgramasMonitoreados { get; set; }
    public int AlertasHoy { get; set; }
    public int AlertasSemana { get; set; }
    public double TiempoPromedioResolucion { get; set; }
}

public class AlertaPorProgramaDto
{
    public int ProgramaId { get; set; }
    public string NombrePrograma { get; set; } = "";
    public int Total { get; set; }
    public int Criticas { get; set; }
    public int Altas { get; set; }
    public int Abiertas { get; set; }
}

public record CambioEstadoArgs(
    int AlertaId, 
    Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common.EstadoAlerta NuevoEstado, 
    byte[] RowVersion
);
