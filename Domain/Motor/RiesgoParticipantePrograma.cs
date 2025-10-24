using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class RiesgoParticipantePrograma : BaseEntity
{
    public int RiesgoId { get; set; }
    public int ParticipanteId { get; set; }
    public int ProgramaId { get; set; }
    public DateTime FechaCorte { get; set; }
    public int Score { get; set; }
    public BandaRiesgo Banda { get; set; }
    public string? ExplicacionCorta { get; set; }

    // Navegación
    public Operacion.Participante Participante { get; set; } = null!;
    public Programas.Programa Programa { get; set; } = null!;
    public ICollection<RiesgoDetalle> Detalles { get; set; } = new List<RiesgoDetalle>();
}
