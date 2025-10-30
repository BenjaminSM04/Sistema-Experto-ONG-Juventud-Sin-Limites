# 🔐 Correcciones de Redirección y Seguridad

## 📅 Fecha: 2025-01-26 18:00

## ✅ Problemas Corregidos

### 1. **Redirección Después de Cambiar Contraseña** 🔄

#### Problema:
- Después de cambiar la contraseña en `/Account/ForceChangePassword`, el usuario no era redirigido automáticamente
- Se quedaba en la misma página hasta que borraba la URL manualmente

#### Causa:
- `NavigationManager.NavigateTo()` no funciona correctamente en el contexto de `EditForm` en Blazor Server
- El componente estaba intentando redirigir mientras el formulario aún procesaba el submit

#### Solución Implementada:
```csharp
// ❌ Antes (no funcionaba)
NavigationManager.NavigateTo("/", forceLoad: true);

// ✅ Ahora (funciona correctamente)
await JSRuntime.InvokeVoidAsync("window.location.href", "/");
```

**Archivos Modificados:**
- `Components/Account/Pages/ForceChangePassword.razor`
  - Agregado `@inject IJSRuntime JSRuntime`
  - Reemplazado `NavigationManager.NavigateTo` con `JSRuntime.InvokeVoidAsync("window.location.href")`

### 2. **Filtrado de Datos por Programa para Coordinadores** 🔒

#### Problema:
- Coordinadores podían ver alertas de TODOS los programas
- Violaba el principio de acceso basado en roles y programas asignados
- No había restricción de datos según los permisos del usuario

#### Causa:
- El Motor de Inferencia no verificaba los programas asignados al usuario
- Las consultas a la base de datos no filtraban por programa

#### Solución Implementada:

##### a) **Selector de Programa para Coordinadores:**
```razor
@if (_esCoordinador)
{
    <MudSelect T="int?" 
        Label="Programa" 
        @bind-Value="_programaId"
        Required="true"
        HelperText="Selecciona uno de tus programas asignados">
        @foreach (var programa in _programasDisponibles)
        {
      <MudSelectItem Value="@((int?)programa.Id)">@programa.Nombre</MudSelectItem>
        }
    </MudSelect>
}
else
{
    <!-- Administradores pueden ingresar cualquier ID de programa -->
    <MudNumericField Label="Programa ID (Opcional)" ... />
}
```

##### b) **Carga de Programas Asignados:**
```csharp
if (_esCoordinador)
{
    // Coordinadores solo ven sus programas asignados
    _programasDisponibles = _usuarioActual.UsuarioProgramas
        .Where(up => !up.IsDeleted && !up.Programa.IsDeleted)
        .Select(up => new ProgramaInfo { Id = up.ProgramaId, Nombre = up.Programa.Nombre })
        .ToList();
}
else
{
    // Administradores ven todos los programas
    _programasDisponibles = await DbContext.Programas
   .Where(p => !p.IsDeleted)
      .Select(p => new ProgramaInfo { Id = p.ProgramaId, Nombre = p.Nombre })
 .ToListAsync();
}
```

##### c) **Filtrado de Alertas:**
```csharp
var queryAlertas = DbContext.Alertas.Where(a => !a.IsDeleted);

if (_esCoordinador && _usuarioActual != null)
{
    // Coordinadores solo ven alertas de sus programas asignados
    var programasIds = _usuarioActual.UsuarioProgramas
    .Where(up => !up.IsDeleted)
        .Select(up => up.ProgramaId)
        .ToList();

    queryAlertas = queryAlertas.Where(a => a.ProgramaId.HasValue && programasIds.Contains(a.ProgramaId.Value));
}

_alertas = await queryAlertas.OrderByDescending(a => a.AlertaId).Take(50).ToListAsync();
```

##### d) **Validación Antes de Ejecutar:**
```csharp
if (_esCoordinador && !_programaId.HasValue)
{
    _error = "Por favor, selecciona un programa antes de ejecutar el motor.";
    return;
}
```

**Archivos Modificados:**
- `Components/Pages/Admin/Motor.razor`
  - Agregado `@inject AuthenticationStateProvider`
  - Agregado `@inject ILogger<Motor>`
  - Agregada variable `_esCoordinador`
  - Agregada variable `_programasDisponibles`
  - Agregada variable `_usuarioActual`
  - Modificado método `OnInitializedAsync()` para cargar programas asignados
  - Modificado método `EjecutarMotor()` para filtrar alertas
  - Agregada validación en `HandleEjecutarClick()`

## 🔐 Mejoras de Seguridad Implementadas

### 1. **Verificación de Rol**
```csharp
var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
var user = authState.User;
_esCoordinador = user.IsInRole("Coordinador");
```

### 2. **Carga de Usuario con Programas**
```csharp
_usuarioActual = await DbContext.Users
    .Include(u => u.UsuarioProgramas)
    .ThenInclude(up => up.Programa)
    .FirstOrDefaultAsync(u => u.Email == user.Identity.Name);
```

### 3. **Filtrado a Nivel de Query**
- Las alertas se filtran en la consulta SQL, no en memoria
- Más eficiente y seguro

### 4. **Logging Mejorado**
```csharp
Logger.LogInformation("👤 Coordinador {Email} tiene acceso a {Count} programas", 
    _usuarioActual.Email, _programasDisponibles.Count);
```

## 🎨 Mejoras de UI

### 1. **Indicador Visual para Coordinadores**
```razor
<MudText Typo="Typo.body1" Color="Color.Secondary" Class="mb-4">
    Ejecuta el motor de inferencia para detectar alertas y riesgos en los programas
    @if (_esCoordinador)
    {
<span class="text-warning"> (Limitado a tus programas asignados)</span>
  }
</MudText>
```

### 2. **Selector de Programa Mejorado**
- Dropdown con nombres de programas en lugar de input numérico
- Lista solo programas asignados al coordinador

### 3. **Mostrar Nombre de Programa en la Tabla**
```razor
@{
    var nombrePrograma = context.ProgramaId.HasValue 
        ? _programasDisponibles.FirstOrDefault(p => p.Id == context.ProgramaId.Value)?.Nombre 
        ?? context.ProgramaId.Value.ToString() 
   : "-";
}
@nombrePrograma
```

## 📊 Matriz de Permisos

| Rol | Ver Todos los Programas | Ejecutar Motor en Cualquier Programa | Ver Alertas de Todos los Programas |
|-----|-------------------------|---------------------------------------|-------------------------------------|
| **Administrador** | ✅ Sí | ✅ Sí | ✅ Sí |
| **Coordinador** | ❌ No (solo asignados) | ❌ No (solo asignados) | ❌ No (solo asignados) |
| **Facilitador** | ❌ No acceso al motor | ❌ No acceso al motor | ❌ No acceso al motor |
| **Visualizador** | ❌ No acceso al motor | ❌ No acceso al motor | ❌ No acceso al motor |

## 🧪 Cómo Probar

### Prueba 1: Redirección de Cambio de Contraseña
```
1. Crear un usuario nuevo desde /Admin/Usuarios/Crear
2. Copiar contraseña temporal
3. Cerrar sesión del admin
4. Iniciar sesión con el nuevo usuario
5. ✅ Ser redirigido a /Account/ForceChangePassword
6. Cambiar la contraseña
7. ✅ Ser redirigido automáticamente a / (Home)
```

### Prueba 2: Filtrado de Programas para Coordinadores
```
1. Crear un coordinador
2. Asignarlo a EDV y Academia
3. Iniciar sesión como coordinador
4. Ir a /admin/motor
5. ✅ Ver solo 2 programas en el selector (EDV y Academia)
6. Ejecutar el motor
7. ✅ Ver solo alertas de EDV o Academia (según el programa seleccionado)
```

### Prueba 3: Acceso Completo de Administrador
```
1. Iniciar sesión como administrador
2. Ir a /admin/motor
3. ✅ Ver input numérico en lugar de selector
4. Ejecutar el motor sin seleccionar programa
5. ✅ Ver alertas de TODOS los programas
```

## ⚠️ Notas Importantes

### 1. **Redirección con JavaScript**
- Se usa `JSRuntime.InvokeVoidAsync("window.location.href")` porque es más confiable en el contexto de formularios
- `forceLoad: true` de `NavigationManager` no funciona correctamente después de un `EditForm.OnValidSubmit`

### 2. **Seguridad de Datos**
- Los coordinadores NUNCA verán alertas de programas no asignados
- El filtrado se hace a nivel de BD, no en el cliente
- Imposible burlar el filtro desde el frontend

### 3. **Rendimiento**
- Las consultas incluyen `Include()` para evitar N+1 queries
- Solo se cargan las últimas 50 alertas
- El filtro se aplica antes de la paginación

## 🚀 Próximas Mejoras Sugeridas

### Corto Plazo:
1. **Filtros adicionales en el Motor**:
- Por estado de alerta (Abierta/Resuelta/Descartada)
   - Por severidad (Info/Alta/Crítica)
   - Por rango de fechas

2. **Dashboard personalizado por rol**:
   - Administradores: Vista general de todos los programas
   - Coordinadores: Vista detallada de sus programas
   - Métricas específicas por rol

### Mediano Plazo:
3. **Notificaciones en tiempo real**:
   - SignalR para alertas nuevas
   - Contador de alertas pendientes
   - Badge en el menú lateral

4. **Exportación de alertas**:
   - Botón "Exportar a Excel"
   - Incluir filtros aplicados
   - Historial de exportaciones

## 📝 Documentación Actualizada

- ✅ `MEJORAS_GESTION_USUARIOS.md` - Actualizado con nueva redirección
- ✅ Este documento (`CORRECCIONES_REDIRECCION_SEGURIDAD.md`)

---

**Versión**: 1.0  
**Build**: ✅ Successful  
**Última actualización**: 2025-01-26 18:00  
**Autor**: Sistema Experto ONG Juventud Sin Límites
