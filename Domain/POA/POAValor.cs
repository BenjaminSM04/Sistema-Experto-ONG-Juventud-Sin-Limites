using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POAValor : BaseEntity
{
 public int ValorId { get; set; }
    public int InstanciaId { get; set; }
    public int CampoId { get; set; }
 public int? ProgramaId { get; set; }
    public int? ActividadId { get; set; }
    public int? ParticipanteId { get; set; }

    public string? ValorTexto { get; set; }
    public int? ValorNumero { get; set; }
    public decimal? ValorDecimal { get; set; }
    public DateTime? ValorFecha { get; set; }
    public bool? ValorBool { get; set; }

    // Navegación
    public POAInstancia Instancia { get; set; } = null!;
    public POACampo Campo { get; set; } = null!;
    public Programas.Programa? Programa { get; set; }
    public Operacion.Actividad? Actividad { get; set; }
    public Operacion.Participante? Participante { get; set; }
}
