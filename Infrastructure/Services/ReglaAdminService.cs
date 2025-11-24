using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Api.Models;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

public class ReglaAdminService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReglaAdminService> _logger;

    public ReglaAdminService(ApplicationDbContext db, ILogger<ReglaAdminService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ReglaDto>> ListarAsync(CancellationToken ct)
    {
        var reglas = await _db.Reglas
            .Include(r => r.Parametros)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Prioridad)
            .ToListAsync(ct);

        return reglas.Select(MapToDto).ToList();
    }

    public async Task<ReglaDto?> ObtenerAsync(int id, CancellationToken ct)
    {
        var r = await _db.Reglas.Include(x => x.Parametros)
            .FirstOrDefaultAsync(x => x.ReglaId == id && !x.IsDeleted, ct);
        return r == null ? null : MapToDto(r);
    }

    public async Task<ReglaDto> UpsertAsync(UpsertReglaRequest request, CancellationToken ct)
    {
        Regla? entidad = null;
        if (request.ReglaId.HasValue)
        {
            entidad = await _db.Reglas.Include(x => x.Parametros)
                .FirstOrDefaultAsync(x => x.ReglaId == request.ReglaId.Value && !x.IsDeleted, ct);
            if (entidad == null)
                throw new InvalidOperationException("Regla no encontrada");
            // Concurrencia
            if (request.RowVersion != null)
                _db.Entry(entidad).Property(e => e.RowVersion).OriginalValue = request.RowVersion;
        }
        else
        {
            entidad = new Regla { CreadoEn = DateTime.UtcNow };
            _db.Reglas.Add(entidad);
        }

        entidad.Clave = request.Clave.Trim();
        entidad.Nombre = request.Nombre.Trim();
        entidad.Descripcion = request.Descripcion?.Trim();
        entidad.Severidad = (Domain.Common.Severidad)request.Severidad;
        entidad.Objetivo = (Domain.Common.ObjetivoRegla)request.Objetivo;
        entidad.Activa = request.Activa;
        entidad.Prioridad = request.Prioridad;
        entidad.Version = request.Version;
        entidad.ActualizadoEn = DateTime.UtcNow;

        // Sincronizar parámetros (borrado lógico si se quitan)
        var existentes = entidad.Parametros.ToDictionary(p => p.ReglaParametroId);
        var idsMantener = new HashSet<int>();
        foreach (var p in request.Parametros)
        {
            ReglaParametro? param = null;
            if (p.ReglaParametroId.HasValue && existentes.TryGetValue(p.ReglaParametroId.Value, out var found))
            {
                param = found;
                if (p.RowVersion != null)
                    _db.Entry(param).Property(x => x.RowVersion).OriginalValue = p.RowVersion;
            }
            else
            {
                param = new ReglaParametro { ReglaId = entidad.ReglaId, CreadoEn = DateTime.UtcNow };
                entidad.Parametros.Add(param);
            }
            param.Nombre = p.Nombre.Trim();
            param.Tipo = (Domain.Common.TipoParametro)p.Tipo;
            param.Valor = p.Valor.Trim();
            param.ActualizadoEn = DateTime.UtcNow;
            idsMantener.Add(param.ReglaParametroId);
        }

        foreach (var param in existentes.Values)
        {
            if (!idsMantener.Contains(param.ReglaParametroId))
            {
                param.IsDeleted = true;
                param.ActualizadoEn = DateTime.UtcNow;
            }
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("La regla fue modificada por otro usuario.");
        }

        // Recargar para devolver DTO actualizado con RowVersion
        await _db.Entry(entidad).ReloadAsync(ct);
        await _db.Entry(entidad).Collection(e => e.Parametros).LoadAsync(ct);
        return MapToDto(entidad);
    }

    public async Task CambiarEstadoAsync(int id, bool activa, byte[]? rowVersion, CancellationToken ct)
    {
        var r = await _db.Reglas.FirstOrDefaultAsync(x => x.ReglaId == id && !x.IsDeleted, ct) ??
            throw new InvalidOperationException("Regla no encontrada");
        if (rowVersion != null)
            _db.Entry(r).Property(e => e.RowVersion).OriginalValue = rowVersion;
        r.Activa = activa;
        r.ActualizadoEn = DateTime.UtcNow;
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("La regla fue modificada por otro usuario.");
        }
    }

    public async Task EliminarAsync(int id, byte[]? rowVersion, CancellationToken ct)
    {
        var r = await _db.Reglas.Include(x => x.Parametros)
            .FirstOrDefaultAsync(x => x.ReglaId == id && !x.IsDeleted, ct) ??
            throw new InvalidOperationException("Regla no encontrada");
        if (rowVersion != null)
            _db.Entry(r).Property(e => e.RowVersion).OriginalValue = rowVersion;
        r.IsDeleted = true;
        r.EliminadoEn = DateTime.UtcNow;
        foreach (var p in r.Parametros)
        {
            p.IsDeleted = true;
            p.EliminadoEn = DateTime.UtcNow;
        }
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("La regla fue modificada por otro usuario.");
        }
    }

    private static ReglaDto MapToDto(Regla r) => new(
        r.ReglaId,
        r.Clave,
        r.Nombre,
        r.Descripcion,
        (byte)r.Severidad,
        (byte)r.Objetivo,
        r.Activa,
        r.Prioridad,
        r.Version,
        r.Parametros.Where(p => !p.IsDeleted)
            .Select(p => new ReglaParametroDto(p.ReglaParametroId, p.Nombre, (byte)p.Tipo, p.Valor, p.RowVersion))
            .ToList(),
        r.RowVersion
    );
}