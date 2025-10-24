# Sistema Experto ONG Juventud Sin Límites - Guía de Base de Datos Code First

## ?? Resumen

Se ha convertido exitosamente la base de datos SQL Server a Code First utilizando Entity Framework Core 8.0.

## ??? Estructura del Proyecto

```
Sistema-Experto-ONG-Juventud-Sin-Limites/
??? Domain/      ? Entidades del dominio (POCOs)
?   ??? Common/        ? Clases base, interfaces y enums
?   ?   ??? BaseEntity.cs
?   ?   ??? IAuditable.cs
?   ?   ??? ISoftDelete.cs
?   ?   ??? Enums.cs
?   ??? Security/            ? Identidad y acceso
?   ?   ??? Persona.cs
? ?   ??? Usuario.cs
?   ?   ??? Rol.cs
?   ?   ??? UsuarioRol.cs
?   ??? Programas/            ? Programas de la ONG
?   ?   ??? Programa.cs
?   ?   ??? UsuarioPrograma.cs
?   ??? Operacion/         ? Operaciones y actividades
?   ?   ??? Participante.cs
?   ?   ??? Actividad.cs
?   ?   ??? ActividadParticipante.cs
?   ?   ??? Asistencia.cs
?   ?   ??? EvidenciaActividad.cs
? ??? POA/              ? Plan Operativo Anual (dinámico)
?   ?   ??? POAPlantilla.cs
?   ?   ??? POAPlantillaSeccion.cs
??   ??? POACampo.cs
?   ?   ??? POACampoOpcion.cs
?   ?   ??? POACampoValidacion.cs
?   ?   ??? POAInstancia.cs
?   ?   ??? POAValor.cs
?   ?   ??? POAArchivo.cs
?   ?   ??? POASnapshotMensual.cs
?   ??? Motor/        ? Motor de inferencia
?   ?   ??? Regla.cs
?   ?   ??? ReglaParametro.cs
?   ?   ??? ReglaParametroOverride.cs
?   ?   ??? Alerta.cs
?   ?   ??? RiesgoParticipantePrograma.cs
?   ?   ??? RiesgoDetalle.cs
?   ?   ??? EjecucionMotor.cs
?   ?   ??? MatchRegla.cs
?   ?   ??? DiccionarioObservaciones.cs
?   ?   ??? DiccionarioObservacionesPrograma.cs
? ??? Config/       ? Configuración del motor
?   ?   ??? ConfiguracionMotor.cs
?   ?   ??? ConfiguracionMotorOverride.cs
?   ??? BI/            ? Business Intelligence
?   ?   ??? MetricasProgramaMes.cs
?   ??? Audit/      ? Auditoría
?    ??? Log.cs
?
??? Infrastructure/     ? Configuración de EF Core
?   ??? Configurations/       ? Fluent API por entidad
?   ?   ??? Security/
?   ?   ??? Programas/
?   ?   ??? Operacion/
?   ?   ??? POA/
?   ?   ??? Motor/
?   ?   ??? Config/
?   ?   ??? BI/
?   ?   ??? Audit/
?   ??? Interceptors/
?       ??? AuditableSaveChangesInterceptor.cs
?
??? Data/
    ??? ApplicationDbContext.cs      ? DbContext principal
  ??? ApplicationUser.cs           ? Usuario de Identity
    ??? Migrations/        ? Migraciones de EF Core
```

## ?? Características Implementadas

### ? Entidades del Dominio
- **47 tablas** convertidas a entidades C#
- Relaciones 1:1, 1:N y N:M configuradas
- Enums para mejorar type-safety

### ? Auditoría Automática
- Campos de auditoría (`CreadoEn`, `ActualizadoEn`, etc.)
- Soft Delete implementado globalmente
- RowVersion para control de concurrencia
- Interceptor que automáticamente gestiona la auditoría

### ? Configuraciones Fluent API
- Configuración separada por entidad
- Índices únicos filtrados por `IsDeleted = 0`
- Precisión decimal configurada
- Tipos de columna SQL específicos

### ? Query Filters Globales
- Filtro automático de `IsDeleted = false` en todas las consultas
- Soporte para restauración de datos eliminados

## ?? Uso de las Migraciones

### 1. Actualizar la Base de Datos

Para aplicar la migración y crear todas las tablas:

```bash
dotnet ef database update --context ApplicationDbContext
```

### 2. Ver las Migraciones Pendientes

```bash
dotnet ef migrations list --context ApplicationDbContext
```

### 3. Generar Script SQL

Para generar un script SQL sin aplicarlo:

```bash
dotnet ef migrations script --context ApplicationDbContext --output migration.sql
```

### 4. Crear una Nueva Migración

Después de modificar las entidades:

```bash
dotnet ef migrations add NombreDeLaMigracion --context ApplicationDbContext --output-dir Data/Migrations
```

### 5. Revertir una Migración

```bash
dotnet ef migrations remove --context ApplicationDbContext
```

### 6. Volver a una Migración Específica

```bash
dotnet ef database update NombreDeLaMigracion --context ApplicationDbContext
```

## ?? Cadena de Conexión

Actualiza `appsettings.json` con tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=POA_JuventudSinLimites;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## ?? Ejemplos de Uso

### Consultar Datos (Soft Delete automático)

```csharp
// Solo trae registros NO eliminados (IsDeleted = false)
var programasActivos = await _context.Programas
    .Where(p => p.Estado == EstadoGeneral.Activo)
    .ToListAsync();

// Para incluir eliminados (ignora el filtro global)
var todosProgramas = await _context.Programas
    .IgnoreQueryFilters()
    .ToListAsync();
```

### Crear un Nuevo Registro

```csharp
var programa = new Programa
{
    Clave = "EDV",
    Nombre = "Escuelas de Valores",
    Descripcion = "Programa de formación en valores",
    Estado = EstadoGeneral.Activo,
    InferenciaActiva = true
};

_context.Programas.Add(programa);
await _context.SaveChangesAsync();
// CreadoEn se establece automáticamente por el interceptor
```

### Soft Delete

```csharp
var programa = await _context.Programas.FindAsync(id);
if (programa != null)
{
    _context.Programas.Remove(programa);
    await _context.SaveChangesAsync();
    // Se marca IsDeleted = true, no se elimina físicamente
}
```

### Restaurar un Registro Eliminado

```csharp
var programa = await _context.Programas
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(p => p.ProgramaId == id && p.IsDeleted);

if (programa != null)
{
    programa.IsDeleted = false;
    programa.EliminadoEn = null;
    programa.EliminadoPorUsuarioId = null;
  await _context.SaveChangesAsync();
}
```

### Consultas con Relaciones

```csharp
// Incluir relaciones
var actividades = await _context.Actividades
    .Include(a => a.Programa)
    .Include(a => a.ActividadParticipantes)
        .ThenInclude(ap => ap.Participante)
    .ThenInclude(p => p.Persona)
    .Where(a => a.Estado == EstadoActividad.Planificada)
    .ToListAsync();
```

## ?? Control de Concurrencia

Todas las entidades tienen `RowVersion` para control de concurrencia optimista:

```csharp
try
{
    programa.Nombre = "Nuevo Nombre";
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Manejar conflicto de concurrencia
    var entry = ex.Entries.Single();
    var databaseValues = await entry.GetDatabaseValuesAsync();
    
    if (databaseValues == null)
    {
        // El registro fue eliminado
    }
  else
    {
        // Resolver el conflicto
        entry.OriginalValues.SetValues(databaseValues);
    }
}
```

## ?? Características Especiales

### Sistema EAV (Entity-Attribute-Value) para POA
El módulo POA utiliza un patrón EAV dinámico que permite:
- Plantillas personalizables por programa
- Campos dinámicos con validaciones
- Histórico de versiones

### Motor de Inferencia
Sistema de reglas configurables para:
- Detección automática de riesgos
- Generación de alertas
- Scoring de participantes
- Overrides por programa

### Métricas y BI
Tablas precalculadas para:
- Métricas mensuales por programa
- Snapshots para análisis histórico
- KPIs de cumplimiento

## ??? Próximos Pasos

1. **Seed Data**: Crear datos iniciales en `Infrastructure/Seed/`
2. **Repositories**: Implementar patrón Repository si es necesario
3. **Services**: Crear servicios de lógica de negocio
4. **DTOs**: Definir objetos de transferencia de datos
5. **Validators**: Implementar validaciones con FluentValidation

## ?? Recursos Adicionales

- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
- [Code First Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [Query Filters](https://docs.microsoft.com/ef/core/querying/filters)
- [Interceptors](https://docs.microsoft.com/ef/core/logging-events-diagnostics/interceptors)

## ?? Notas Importantes

1. **Backup**: Siempre haz backup de tu base de datos antes de aplicar migraciones en producción
2. **Testing**: Prueba las migraciones en un ambiente de desarrollo primero
3. **Índices**: Los índices únicos filtrados requieren SQL Server 2008+
4. **Performance**: Considera deshabilitar el soft delete filter en consultas pesadas si es necesario
5. **Identity**: El sistema convive con ASP.NET Identity para autenticación web

---

**Última actualización**: $(Get-Date -Format "yyyy-MM-dd")
**Versión EF Core**: 8.0.21
**Target Framework**: .NET 8
