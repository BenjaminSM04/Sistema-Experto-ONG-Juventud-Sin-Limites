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
}

public record CambioEstadoArgs(
    int AlertaId, 
    Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common.EstadoAlerta NuevoEstado, 
    byte[] RowVersion
);
