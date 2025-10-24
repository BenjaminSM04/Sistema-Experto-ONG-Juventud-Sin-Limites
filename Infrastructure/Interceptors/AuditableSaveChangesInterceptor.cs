using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Interceptors;

public class AuditableSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
  return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
     InterceptionResult<int> result,
CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
     if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
  switch (entry.State)
    {
   case EntityState.Added:
      entry.Entity.CreadoEn = DateTime.UtcNow;
              // Aquí podrías asignar CreadoPorUsuarioId desde el contexto HTTP
   break;

                case EntityState.Modified:
 entry.Entity.ActualizadoEn = DateTime.UtcNow;
     // Aquí podrías asignar ActualizadoPorUsuarioId desde el contexto HTTP
     break;
   }
    }

        // Manejo de Soft Delete
        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
        if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
       entry.Entity.IsDeleted = true;
      entry.Entity.EliminadoEn = DateTime.UtcNow;
   // Aquí podrías asignar EliminadoPorUsuarioId
            }
        }
    }
}
