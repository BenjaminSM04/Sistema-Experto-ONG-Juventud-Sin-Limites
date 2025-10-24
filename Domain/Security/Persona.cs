using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

public class Persona : BaseEntity
{
    public int PersonaId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }

// Navegación
    public Usuario? Usuario { get; set; }
    public Operacion.Participante? Participante { get; set; }
}
