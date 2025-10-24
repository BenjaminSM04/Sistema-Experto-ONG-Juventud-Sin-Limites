using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.BI;

public class MetricasProgramaMes : BaseEntity
{
    public int ProgramaId { get; set; }
    public string AnioMes { get; set; } = string.Empty; // 'YYYY-MM'
    public int ActividadesPlanificadas { get; set; }
    public int ActividadesEjecutadas { get; set; }
    public decimal PorcCumplimiento { get; set; }
    public decimal RetrasoPromedioDias { get; set; }
    public decimal PorcAsistenciaProm { get; set; }

    // Navegaci�n
    public Programas.Programa Programa { get; set; } = null!;
}
