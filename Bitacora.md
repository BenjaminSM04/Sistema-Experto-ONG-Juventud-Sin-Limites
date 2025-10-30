# 📝 Bitácora de Desarrollo

## Registro de Cambios y Soluciones

---

## 2025-01-26 - Configuración de Tests y Sistema de Permisos

### ✅ Configuración de Tests

**Problema**: Tests se ejecutaban automáticamente al iniciar la aplicación

**Solución Implementada**:
1. **Proyecto de Tests Correctamente Configurado**
   - Agregado `<IsTestProject>true</IsTestProject>` al archivo .csproj
   - Agregado `<IsPackable>false</IsPackable>`
   - Configurado `RootNamespace` correcto

2. **Exclusión Explícita en Proyecto Principal**
   ```xml
   <ItemGroup>
 <Compile Remove="Tests\**" />
       <Content Remove="Tests\**" />
  <EmbeddedResource Remove="Tests\**" />
   <None Remove="Tests\**" />
   </ItemGroup>
   ```

3. **Tests Solo se Ejecutan Manualmente**
 - Con Test Explorer en Visual Studio
   - Con `dotnet test` desde terminal
   - Nunca al hacer `dotnet run` o F5

**Comandos de Tests**:
```bash
# Todos los tests
dotnet test

# Tests específicos
dotnet test --filter "FullyQualifiedName~MotorInferenciaTests"

# Con cobertura
dotnet test /p:CollectCoverage=true
```

---

## 2025-01-26 - Sistema de Roles y Permisos Granulares

### 🎯 Planificación: Sistema Multi-Rol con Control de Acceso

#### Roles Definidos

1. **Administrador**
   - ✅ Acceso total al sistema
   - ✅ Ve todos los programas
   - ✅ Acceso a dashboards completos (Metabase)
   - ✅ Ve información sensible (presupuesto, finanzas, datos completos)
   - ✅ Gestión de usuarios y roles
   - ✅ Configuración del sistema

2. **Coordinador**
   - ✅ Acceso solo a programas asignados
   - ✅ Dashboards filtrados por programa
   - 🔒 NO ve presupuesto ni información financiera
   - ✅ Ve asistencia y participación
   - ✅ Puede generar alertas manualmente
 - ✅ Gestión de actividades de su programa
   - ✅ Gestión de participantes de su programa

3. **Facilitador**
   - ✅ Acceso solo a actividades asignadas
   - ✅ Registro de asistencia
   - ✅ Ve participantes de sus actividades
   - 🔒 NO ve dashboards completos
   - 🔒 NO ve información financiera
   - ✅ Subir evidencias de actividades

4. **Visualizador**
   - ✅ Solo lectura
   - ✅ Ve datos públicos de programas
   - 🔒 NO ve información sensible
   - 🔒 NO puede editar nada

#### Arquitectura de Permisos

```
┌─────────────────────────────────────────────┐
│           Feature-Based Security    │
├─────────────────────────────────────────────┤
│  IFeatureProvider       │
│  └─ CanAccess(feature, programaId?, ...)   │
│         │
│  Features:         │
│  - Ver.Presupuesto       │
│  - Ver.Finanzas          │
│  - Ver.Dashboard.Completo       │
│  - Ver.Asistencia    │
│  - Editar.Programa     │
│  - Gestionar.Usuarios            │
│  - Ejecutar.Motor           │
└─────────────────────────────────────────────┘
```

#### Filtrado de Datos por Rol

**Administrador**:
```csharp
// Sin filtros, ve todo
var programas = await _context.Programas.ToListAsync();
```

**Coordinador**:
```csharp
// Solo programas asignados
var programas = await _context.UsuarioProgramas
    .Where(up => up.UsuarioId == currentUserId)
    .Select(up => up.Programa)
    .ToListAsync();
```

**Facilitador**:
```csharp
// Solo actividades donde es facilitador
var actividades = await _context.Actividades
    .Where(a => a.FacilitadorId == currentUserId)
    .ToListAsync();
```

#### Campos Sensibles por Modelo

| Modelo | Campos Sensibles (Solo Admin) | Campos Públicos |
|--------|------------------------------|-----------------|
| Programa | Presupuesto, CostoTotal | Nombre, Descripcion, Estado |
| Actividad | CostoActividad | Titulo, Fecha, Lugar |
| Participante | DatosSensibles* | Nombres, FechaAlta |
| POA | PresupuestoAsignado | ActividadesPlanificadas |
| Alertas | - | Todas (según rol) |

*DatosSensibles: Información médica, familiar, económica

### 📋 Plan de Implementación

#### Fase 1: Infraestructura de Permisos ✅ (Completado parcialmente)
- [x] IFeatureProvider básico
- [ ] Expandir Features disponibles
- [ ] Policy-based authorization
- [ ] Claims personalizados por programa

#### Fase 2: Servicios de Datos Filtrados
- [ ] Crear interfaces de repositorio
- [ ] Implementar filtros por rol
- [ ] DTOs con campos condicionales
- [ ] Mapper con permisos

#### Fase 3: UI Adaptativa
- [ ] Componentes que se adaptan al rol
- [ ] Menú dinámico según permisos
- [ ] Dashboard principal por rol
- [ ] Breadcrumbs y navegación contextual

#### Fase 4: Dashboards con Metabase
- [ ] Integración con Metabase
- [ ] Filtros automáticos por rol
- [ ] Embed de dashboards seguros
- [ ] Row-level security (RLS)

#### Fase 5: Módulos por Rol

**Dashboard Administrador**:
- Visión general de todos los programas
- Métricas financieras
- Estado del motor de inferencia
- Gestión de usuarios
- Reportes completos

**Dashboard Coordinador**:
- Programas asignados
- Métricas de asistencia y participación
- Alertas de su programa
- Gestión de actividades
- Participantes activos

**Dashboard Facilitador**:
- Actividades asignadas
- Asistencia de hoy/semana
- Lista de participantes
- Subir evidencias

**Dashboard Visualizador**:
- Reportes públicos
- Estadísticas generales
- Calendario de actividades

### 🔐 Implementación de Features

```csharp
public enum Feature
{
    // Visualización
    Ver_Dashboard_Completo,
    Ver_Dashboard_Programa,
    Ver_Dashboard_Actividad,
    Ver_Presupuesto,
    Ver_Finanzas,
    Ver_DatosSensibles,
    Ver_Asistencia,
    Ver_Participantes,
    Ver_Alertas,
Ver_Metricas,
    
    // Gestión
    Gestionar_Usuarios,
    Gestionar_Roles,
    Gestionar_Programas,
    Gestionar_Actividades,
    Gestionar_Participantes,
    Gestionar_POA,
    
 // Motor
    Ejecutar_Motor,
    Configurar_Motor,
    Ver_Logs_Motor,
    
// Reportes
    Generar_Reportes_Completos,
    Generar_Reportes_Programa,
    Exportar_Datos,
    
    // Metabase
    Acceso_Metabase_Admin,
    Acceso_Metabase_Coordinador,
    Acceso_Metabase_Lectura
}
```

### 🎨 Diseño de Navegación por Rol

**Menú Administrador**:
```
📊 Dashboard
├─ 📈 Métricas Generales
├─ 💰 Finanzas
└─ 🎯 KPIs

📚 Programas
├─ ➕ Nuevo Programa
├─ 📋 Lista Programas
└─ 📊 Reportes

👥 Participantes
├─ ➕ Nuevo Participante
├─ 📋 Lista Participantes
└─ 📊 Estadísticas

📅 Actividades
├─ ➕ Nueva Actividad
├─ 📋 Calendario
└─ ✅ Asistencias

🤖 Motor de Inferencia
├─ ⚙️ Configuración
├─ ▶️ Ejecutar
├─ ⚠️ Alertas
└─ 📜 Historial

👤 Usuarios
├─ ➕ Nuevo Usuario
├─ 📋 Lista Usuarios
└─ 🔐 Roles

📊 Metabase
└─ 🔗 Dashboards Completos
```

**Menú Coordinador**:
```
📊 Mi Dashboard
└─ 📈 Métricas de Mis Programas

📚 Mis Programas
├─ 📋 [Programa EDV]
├─ 📋 [Programa ACADEMIA]
└─ 📊 Reportes

👥 Participantes
├─ ➕ Inscribir Participante
└─ 📋 Lista Participantes

📅 Actividades
├─ ➕ Nueva Actividad
├─ 📋 Mis Actividades
└─ ✅ Registrar Asistencia

⚠️ Alertas
└─ 📋 Alertas de Mis Programas

📊 Reportes
└─ 📈 Asistencia y Participación
```

**Menú Facilitador**:
```
📅 Mis Actividades
├─ 📋 Próximas Actividades
└─ ✅ Asistencia de Hoy

👥 Participantes
└─ 📋 Lista de Mis Actividades

📸 Evidencias
└─ ⬆️ Subir Fotos/Videos
```

### 🔄 Próximos Pasos Inmediatos

1. **Expandir IFeatureProvider**
   - Agregar más features
   - Implementar cache de permisos
   - Logging de accesos

2. **Crear Servicios Base**
   - ProgramaService con filtros
   - ParticipanteService con filtros
   - ActividadService con filtros

3. **Implementar DTOs Condicionales**
   - ProgramaDto (con/sin presupuesto)
   - ParticipanteDto (con/sin datos sensibles)
   - Mapper con permisos

4. **Crear Componentes Base de UI**
   - DashboardBase.razor
   - MenuPrincipal.razor (dinámico)
   - Breadcrumb.razor

5. **Integración Metabase**
   - Setup de Metabase
   - Configuración de RLS
   - Embedding seguro

---

## 2025-01-26 - Motor de Inferencia y Corrección de Errores

### ✅ Logros del Día

1. **Motor de Inferencia Funcional**
   - Se implementó completamente el motor de inferencia
   - Se ejecuta correctamente y genera alertas
   - Se verificó con datos de prueba

2. **Solución MudPopoverProvider**
   - **Problema**: Error "Missing <MudPopoverProvider />"
   - **Causa**: Conflictos con la configuración de MudBlazor providers
   - **Solución**: 
     - Eliminamos los providers de `App.razor`
     - Los colocamos en `MainLayout.razor`
     - Usamos formato self-closing tags: `<MudPopoverProvider />`
     - Simplificamos componentes que requerían popover (MudDatePicker → MudTextField, eliminamos MudTooltip)

3. **Solución Llamada HTTP vs Servicio Directo**
   - **Problema**: Error de serialización JSON al llamar API vía HTTP
   - **Causa**: Posible problema de autenticación/cookies en peticiones HTTP desde Blazor
   - **Solución**: 
     - Cambiamos de llamada HTTP a inyección directa del servicio
     - `@inject IMotorInferencia MotorInferencia`
     - `@inject ApplicationDbContext DbContext`
   - Llamada directa: `await MotorInferencia.EjecutarAsync(...)`
     - **Ventajas**: Más eficiente, sin overhead HTTP, sin problemas de autenticación

4. **Página Motor.razor Completamente Funcional**
   - Botón "Ejecutar Ahora" funciona correctamente
   - Muestra resumen de ejecución (reglas ejecutadas, alertas generadas, errores)
   - Tabla de alertas con acciones (Resolver, Descartar)
   - Mensajes de éxito/error personalizados
 - Todo sin dependencias de MudPopoverProvider

### 🐛 Errores Encontrados y Solucionados

#### Error 1: MudPopoverProvider Missing
```
System.InvalidOperationException: Missing <MudPopoverProvider />
```
**Solución**: Reubicación de providers en MainLayout.razor con formato self-closing

#### Error 2: JSON Serialization Error
```
'<' is an invalid start of a value
```
**Solución**: Cambio de HTTP client a inyección directa de servicios

#### Error 3: Duplicate MudServices Registration
```
AddMudServices() called twice
```
**Solución**: Eliminación de llamada duplicada en Program.cs

### 📚 Aprendizajes

1. **Blazor Server Best Practices**
   - Preferir inyección de servicios sobre llamadas HTTP en Blazor Server
   - Los providers de UI deben estar en el layout, no en App.razor
   - Usar self-closing tags para componentes que no aceptan ChildContent

2. **MudBlazor**
   - MudDatePicker, MudTooltip, MudSnackbar requieren MudPopoverProvider
   - Alternativas simples: MudTextField para fechas, MudButton con texto descriptivo
   - Version 8.13.0 tiene peculiaridades con nested providers

3. **Entity Framework Core**
   - Inyección directa de DbContext es válida y eficiente en Blazor Server
   - Las consultas LINQ se ejecutan de forma asíncrona
   - Usar `.Include()` para cargar relaciones cuando sea necesario

### 🔧 Configuraciones Importantes

```csharp
// Program.cs - Servicios clave
builder.Services.AddScoped<IMotorInferencia, MotorInferencia>();
builder.Services.AddScoped<IFeatureProvider, FeatureProvider>();
builder.Services.AddHostedService<MotorScheduler>();
builder.Services.AddMudServices(); // ¡Solo una vez!
```

```razor
<!-- MainLayout.razor - Providers de MudBlazor -->
@using MudBlazor
@inherits LayoutComponentBase

<MudThemeProvider />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />

<div class="page">
    @Body
</div>
```

### 📊 Estado del Proyecto

#### ✅ Completado
- [x] Conversión de SQL a Code First
- [x] Integración de ASP.NET Core Identity
- [x] Motor de Inferencia funcional
- [x] Página de administración del motor
- [x] Generación de alertas
- [x] Seeder de datos de prueba
- [x] Sistema de auditoría
- [x] Soft Delete global

#### ⏳ En Progreso
- [ ] Interfaces de gestión de programas
- [ ] Interfaces de gestión de participantes
- [ ] Módulos de POA
- [ ] Dashboard de métricas

#### 📋 Pendiente
- [ ] Testing completo
- [ ] Reportes y exportación
- [ ] Deploy a producción
- [ ] Documentación de API

### 🎯 Próximos Pasos

1. Limpiar archivos `.md` innecesarios (mantener solo README.md, Desarrolladores.md, Bitacora.md)
2. Implementar interfaces de gestión de programas
3. Crear dashboards con métricas del motor
4. Agregar más reglas al motor de inferencia
5. Implementar notificaciones por email

---

## Notas Técnicas

### Comandos Útiles Ejecutados Hoy

```bash
# Build exitoso
dotnet build

# Aplicar migraciones
dotnet ef database update --context ApplicationDbContext

# Ver logs
# (En Visual Studio Output Window - ASP.NET Core Web Server)
```

### Archivos Clave Modificados

1. `Components/Pages/Admin/Motor.razor` - Página principal del motor
2. `Components/Layout/MainLayout.razor` - Layout con providers de MudBlazor
3. `Components/App.razor` - Simplificado, sin providers
4. `Program.cs` - Corrección de servicios duplicados

---

**Última actualización**: 2025-01-26 16:55

---

## 2025-01-29 - Sistema de Gestión de Usuarios Implementado

### ✅ Implementación Completada

1. **Modelo de Datos Extendido**
   - Agregadas columnas `MustChangePassword`, `CreatedBy`, `CreatedAtUtc` a `Usuario`
 - Migración aplicada exitosamente

2. **Middleware de Seguridad**
   - `ForceChangePasswordMiddleware` implementado
   - Bloquea acceso hasta cambio de contraseña
   - Rutas permitidas configuradas

3. **Páginas Implementadas**
   - `/Account/ForceChangePassword` - Cambio obligatorio
   - `/Admin/Usuarios` - Gestión de usuarios (MudBlazor)
   - `CreateUserDialog` - Diálogo de creación

4. **Seguridad**
   - Contraseñas: mínimo 12 caracteres, complejidad obligatoria
   - Lockout: 5 intentos, 15 minutos
   - Generación segura de contraseñas temporales
   - Sin registro público

5. **Documentación**
   - `IMPLEMENTACION_GESTION_USUARIOS.md` creado
   - Script SQL manual: `Scripts/AddPasswordManagementFields.sql`
   - Script de verificación: `Scripts/VerificarSeeding.sql`

### 🐛 Problema: Usuario Admin No Aparece en la Base de Datos

**Síntomas:**
- Seeder reporta "✅ Seeding completado exitosamente"
- `SELECT * FROM Usuario` devuelve 0 filas
- Login falla con error de credenciales

**SOLUCIÓN ENCONTRADA ✅:**

El error real era: **"Passwords must be at least 12 characters"**

La contraseña original `Admin@123` solo tenía **9 caracteres**, pero las políticas configuradas requieren **mínimo 12 caracteres**.

**Nueva contraseña:** `Admin@2025!` (12 caracteres)

**Requisitos de Contraseñas:**
- ✅ Mínimo 12 caracteres
- ✅ Al menos 1 letra mayúscula (A-Z)
- ✅ Al menos 1 letra minúscula (a-z)  
- ✅ Al menos 1 número (0-9)
- ✅ Al menos 1 carácter especial (!@#$%^&*)
- ✅ Mínimo 4 caracteres únicos

**Archivos Modificados:**
1. `Infrastructure/Seed/DatabaseSeeder.cs` - Contraseña actualizada a `Admin@2025!`
2. `apply-identity-integration.ps1` - Script actualizado con nueva contraseña y requisitos
3. `README.md` - Documentación actualizada

**Para Aplicar los Cambios:}

```bash
# 1. Limpiar y recrear la base de datos
powershell -ExecutionPolicy Bypass -File apply-identity-integration.ps1

# 2. Ejecutar la aplicación
dotnet run

# 3. Login con nuevas credenciales
#    Email: admin@ong.com
#    Password: Admin@2025!
```

### 📝 Credenciales Actualizadas

```
Email:    admin@ong.com
Password: Admin@2025!
Rol:      Administrador
```

### ✅ Estado Final

- [x] Usuario admin se crea correctamente
- [x] Contraseña cumple políticas de seguridad
- [x] Login funciona correctamente
- [x] MustChangePassword = false para admin
- [x] Documentación actualizada
