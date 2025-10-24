# ?? Sistema Experto ONG Juventud Sin L�mites

## ?? Descripci�n del Proyecto

Sistema integral para la gesti�n de programas, actividades, participantes y planes operativos anuales (POA) de la ONG Juventud Sin L�mites, con motor de inferencia para detecci�n de riesgos y alertas autom�ticas.

---

## ?? CREDENCIALES DE ACCESO

### Usuario Administrador por Defecto

```
Email:    admin@ong.com
Password: Admin@123
Rol:      Administrador
```

?? **IMPORTANTE:** Cambiar estas credenciales en producci�n.

---

## ?? Inicio R�pido

### Prerrequisitos

- .NET 8 SDK
- SQL Server LocalDB (o SQL Server)
- Visual Studio 2022 / VS Code / Rider

### Instalaci�n y Configuraci�n

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/BenjaminSM04/Sistema-Experto-ONG-Juventud-Sin-Limites.git
   cd Sistema-Experto-ONG-Juventud-Sin-Limites
   ```

2. **Restaurar paquetes**
   ```bash
   dotnet restore
   ```

3. **Actualizar cadena de conexi�n** (si es necesario)
   
   Edita `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=POA_JuventudSinLimites;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Aplicar migraciones**
   ```bash
   dotnet ef database update --context ApplicationDbContext
   ```

5. **Ejecutar la aplicaci�n**
   ```bash
   dotnet run
 ```

6. **Acceder a la aplicaci�n**
   - URL: `https://localhost:5001` o `http://localhost:5000`
   - Login con: `admin@ong.com` / `Admin@123`

---

## ??? Arquitectura del Proyecto

### Estructura de Carpetas

```
Sistema-Experto-ONG-Juventud-Sin-Limites/
??? Components/              # Componentes Blazor
?   ??? Account/          # Componentes de autenticaci�n
?   ??? Layout/             # Layouts de la aplicaci�n
???? Pages/         # P�ginas Razor
?
??? Domain/         # Capa de Dominio (POCOs)
?   ??? Common/            # Clases base, interfaces y enums
?   ??? Security/      # Identidad y acceso
?   ??? Programas/         # Programas de la ONG
?   ??? Operacion/         # Operaciones y actividades
?   ??? POA/           # Plan Operativo Anual (din�mico)
?   ??? Motor/   # Motor de inferencia
?   ??? Config/            # Configuraci�n del motor
? ??? BI/          # Business Intelligence
?   ??? Audit/             # Auditor�a
?
??? Infrastructure/        # Infraestructura de EF Core
?   ??? Configurations/    # Configuraciones Fluent API
?   ??? Interceptors/    # Interceptores de EF Core
?   ??? Extensions/        # M�todos de extensi�n
?   ??? Seed/       # Datos iniciales
?
??? Data/          # DbContext y migraciones
?   ??? Migrations/        # Migraciones de EF Core
?
??? wwwroot/  # Archivos est�ticos
```

---

## ?? Base de Datos

### Estad�sticas

- **Total de tablas:** 54
- **Tablas de Identity:** 7
- **Tablas del Dominio:** 47
- **�ndices:** 40+
- **Relaciones:** 50+

### M�dulos de la Base de Datos

#### ?? Security (4 tablas + 4 de Identity)
- `Persona` - Datos personales
- `Usuario` - Usuarios del sistema (integrado con IdentityUser)
- `Rol` - Roles del sistema (integrado con IdentityRole)
- `UsuarioRol` - Relaci�n Usuario-Rol
- `UsuarioClaim`, `UsuarioLogin`, `RolClaim`, `UsuarioToken` - Tablas de Identity

#### ?? Programas (2 tablas)
- `Programa` - Programas de la ONG (EDV, Academia, etc.)
- `UsuarioPrograma` - Asignaci�n de usuarios a programas

#### ?? Operacion (5 tablas)
- `Participante` - Participantes de los programas
- `Actividad` - Actividades de los programas
- `ActividadParticipante` - Inscripci�n a actividades
- `Asistencia` - Control de asistencia
- `EvidenciaActividad` - Evidencias (fotos, videos, actas)

#### ?? POA - Plan Operativo Anual (9 tablas - Sistema EAV)
- `POA_Plantilla` - Plantillas de POA por programa
- `POA_PlantillaSeccion` - Secciones de plantillas
- `POA_Campo` - Campos din�micos
- `POA_CampoOpcion` - Opciones para campos tipo lista
- `POA_CampoValidacion` - Validaciones de campos
- `POA_Instancia` - Instancias de POA (mensual/anual)
- `POA_Valor` - Valores de campos (EAV)
- `POA_Archivo` - Archivos adjuntos
- `POA_SnapshotMensual` - Snapshots para BI

#### ?? Motor de Inferencia (10 tablas)
- `Regla` - Reglas del motor
- `ReglaParametro` - Par�metros de reglas
- `ReglaParametroOverride` - Override por programa
- `Alerta` - Alertas generadas
- `RiesgoParticipantePrograma` - Scoring de riesgo
- `RiesgoDetalle` - Detalles del c�lculo de riesgo
- `EjecucionMotor` - Log de ejecuciones
- `MatchRegla` - Matches de reglas
- `DiccionarioObservaciones` - Diccionario de t�rminos
- `DiccionarioObservacionesPrograma` - Relaci�n con programas

#### ?? Configuraci�n (2 tablas)
- `ConfiguracionMotor` - Configuraci�n global
- `ConfiguracionMotorOverride` - Override por programa

#### ?? BI - Business Intelligence (1 tabla)
- `MetricasProgramaMes` - M�tricas precalculadas mensuales

#### ?? Auditor�a (1 tabla)
- `Logs` - Logs detallados de auditor�a

---

## ?? Datos Iniciales (Seeder)

Al ejecutar la aplicaci�n por primera vez, se cargan autom�ticamente:

### Roles (4)
- **Administrador** - Acceso total al sistema
- **Coordinador** - Coordinador de programas
- **Facilitador** - Facilitador de actividades
- **Visualizador** - Solo lectura

### Usuario Administrador (1)
- **Email:** `admin@ong.com`
- **Password:** `Admin@123`
- **Rol:** Administrador
- **Persona:** Administrador Del Sistema

### Programas (4)
- **EDV** - Escuelas de Valores
- **ACADEMIA** - Academia de Liderazgo
- **JUVENTUD_SEGURA** - Juventud Segura
- **BERNABE** - Programa Bernab�

### Configuraciones del Motor (6)
- `ASISTENCIA_MIN_PORCENTAJE`: 75%
- `DIAS_ALERTA_INASISTENCIA`: 7 d�as
- `UMBRAL_RIESGO_BAJO`: 30 puntos
- `UMBRAL_RIESGO_MEDIO`: 60 puntos
- `MOTOR_EJECUCION_AUTO`: true
- `FRECUENCIA_EJECUCION_HORAS`: 24 horas

### Reglas del Motor (5)
1. **INASISTENCIA_CONSECUTIVA** - Alta prioridad
2. **BAJA_ASISTENCIA_GENERAL** - Alta prioridad
3. **ACTIVIDAD_SIN_ASISTENTES** - Info
4. **RETRASO_ACTIVIDAD** - Alta prioridad
5. **BAJO_CUMPLIMIENTO_POA** - Cr�tica

---

## ?? Caracter�sticas Principales

### ? Autenticaci�n y Autorizaci�n
- ? ASP.NET Core Identity integrado
- ? Login con email y password
- ? Roles y pol�ticas de autorizaci�n
- ? Recuperaci�n de contrase�a
- ? Confirmaci�n de email
- ? Two-Factor Authentication (preparado)
- ? Logins externos (Google, Facebook) - preparado
- ? Lockout autom�tico

### ? Seguridad
- ? Password hashing (PBKDF2)
- ? Tokens seguros para reset de password
- ? Protecci�n CSRF autom�tica
- ? Control de concurrencia optimista (RowVersion)

### ? Auditor�a Autom�tica
- ? CreadoEn, CreadoPorUsuarioId
- ? ActualizadoEn, ActualizadoPorUsuarioId
- ? EliminadoEn, EliminadoPorUsuarioId
- ? Logs detallados en tabla Logs

### ? Soft Delete Global
- ? Eliminaci�n l�gica en todas las entidades
- ? Query filters autom�ticos
- ? �ndices �nicos filtrados por IsDeleted

### ? Sistema POA Din�mico (EAV)
- ? Plantillas personalizables por programa
- ? Campos din�micos con validaciones
- ? Soporta m�ltiples tipos de datos
- ? Versiones de plantillas
- ? Snapshots mensuales para BI

### ? Motor de Inferencia
- ? Reglas configurables
- ? Par�metros personalizables por programa
- ? Detecci�n autom�tica de riesgos
- ? Generaci�n de alertas
- ? Scoring de participantes
- ? Log de ejecuciones

### ? Business Intelligence
- ? M�tricas precalculadas mensuales
- ? Snapshots hist�ricos
- ? KPIs de cumplimiento
- ? An�lisis de asistencia

---

## ??? Tecnolog�as Utilizadas

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core Identity** - Autenticaci�n y autorizaci�n
- **Entity Framework Core 8.0.21** - ORM
- **SQL Server** - Base de datos

### Frontend
- **Blazor Server** - Framework de UI
- **MudBlazor 8.13.0** - Componentes de UI
- **Razor Components** - Componentes interactivos

### Otros
- **Newtonsoft.Json** - Serializaci�n JSON
- **Code First Migrations** - Gesti�n de esquema de BD

---

## ?? Comandos �tiles

### Entity Framework

```bash
# Ver migraciones
dotnet ef migrations list --context ApplicationDbContext

# Crear migraci�n
dotnet ef migrations add NombreMigracion --context ApplicationDbContext

# Aplicar migraciones
dotnet ef database update --context ApplicationDbContext

# Eliminar �ltima migraci�n
dotnet ef migrations remove --context ApplicationDbContext

# Generar script SQL
dotnet ef migrations script --context ApplicationDbContext --output migration.sql

# Ver info del contexto
dotnet ef dbcontext info --context ApplicationDbContext

# Eliminar base de datos
dotnet ef database drop --context ApplicationDbContext --force
```

### Build y Ejecuci�n

```bash
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Ejecutar en modo watch
dotnet watch run

# Publicar
dotnet publish --configuration Release
```

---

## ?? Documentaci�n Adicional

### Gu�as Detalladas
- **DATABASE_CODE_FIRST_README.md** - Gu�a completa de Code First
- **IDENTITY_INTEGRATION.md** - Gu�a de integraci�n de Identity
- **IDENTITY_INTEGRATION_SUCCESS.md** - Resumen de �xito de Identity
- **RESUMEN_IMPLEMENTACION.md** - Resumen ejecutivo
- **COMANDOS_UTILES.md** - Comandos frecuentes
- **CHECKLIST_VERIFICACION.md** - Lista de verificaci�n
- **CHECKLIST_IDENTITY_VERIFICATION.md** - Verificaci�n de Identity

### Scripts
- **apply-identity-integration.ps1** - Script para aplicar Identity

---

## ?? Flujo de Trabajo de Desarrollo

### 1. Modificar Entidades (Domain/)
```csharp
// Agregar nueva propiedad
public class Participante : BaseEntity
{
    // ...propiedades existentes...
    public string? NumeroDocumento { get; set; } // Nueva
}
```

### 2. Actualizar Configuraci�n (Infrastructure/Configurations/)
```csharp
public class ParticipanteConfig : IEntityTypeConfiguration<Participante>
{
    public void Configure(EntityTypeBuilder<Participante> builder)
    {
        // ...configuraci�n existente...
        builder.Property(p => p.NumeroDocumento)
            .HasMaxLength(20);
    }
}
```

### 3. Crear Migraci�n
```bash
dotnet ef migrations add AgregarNumeroDocumentoParticipante --context ApplicationDbContext
```

### 4. Aplicar Migraci�n
```bash
dotnet ef database update --context ApplicationDbContext
```

---

## ?? Ejemplos de Uso

### Crear Usuario con Identity

```csharp
@inject UserManager<Usuario> UserManager
@inject ApplicationDbContext Context

// Crear persona
var persona = new Persona
{
 Nombres = "Juan",
 Apellidos = "P�rez",
    FechaNacimiento = new DateTime(1995, 5, 15),
    Telefono = "1234-5678"
};
Context.Personas.Add(persona);
await Context.SaveChangesAsync();

// Crear usuario
var usuario = new Usuario
{
    PersonaId = persona.PersonaId,
    UserName = "juan.perez@ong.com",
    Email = "juan.perez@ong.com",
    EmailConfirmed = true,
    Estado = EstadoGeneral.Activo
};

// Crear con password
var result = await UserManager.CreateAsync(usuario, "Password123!");

if (result.Succeeded)
{
    // Asignar rol
    await UserManager.AddToRoleAsync(usuario, "Facilitador");
}
```

### Consultar con Soft Delete Autom�tico

```csharp
// Solo trae registros NO eliminados
var programas = await _context.Programas
    .Where(p => p.Estado == EstadoGeneral.Activo)
    .ToListAsync();

// Para incluir eliminados
var todosProgramas = await _context.Programas
    .IgnoreQueryFilters()
    .ToListAsync();
```

### Verificar Roles en Blazor

```razor
<AuthorizeView Roles="Administrador">
  <Authorized>
        <AdminPanel />
    </Authorized>
    <NotAuthorized>
  <p>No tienes permisos de administrador</p>
    </NotAuthorized>
</AuthorizeView>
```

---

## ?? Testing

### Verificar que la BD se cre� correctamente

```sql
USE POA_JuventudSinLimites;
GO

-- Ver todas las tablas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE='BASE TABLE' 
ORDER BY TABLE_NAME;

-- Verificar datos iniciales
SELECT * FROM Rol WHERE IsDeleted = 0;
SELECT * FROM Programa WHERE IsDeleted = 0;
SELECT * FROM ConfiguracionMotor WHERE IsDeleted = 0;
SELECT * FROM Regla WHERE IsDeleted = 0;
```

### Probar Login
1. Ejecuta `dotnet run`
2. Navega a `https://localhost:5001/Account/Login`
3. Ingresa: `admin@ong.com` / `Admin@123`
4. Deber�as iniciar sesi�n exitosamente

---

## ?? Estado del Proyecto

### ? Completado
- [x] Conversi�n de SQL a Code First (47 tablas)
- [x] Integraci�n de ASP.NET Core Identity (7 tablas)
- [x] Auditor�a autom�tica
- [x] Soft Delete global
- [x] Seeder de datos iniciales
- [x] Migraciones aplicadas
- [x] Documentaci�n completa

### ? En Progreso
- [ ] Interfaces de usuario personalizadas
- [ ] L�gica de negocio del motor de inferencia
- [ ] M�dulos de gesti�n de programas
- [ ] M�dulos de gesti�n de participantes
- [ ] M�dulos de POA

### ?? Pendiente
- [ ] Testing unitario
- [ ] Testing de integraci�n
- [ ] Implementaci�n real de env�o de emails
- [ ] Configuraci�n de proveedores externos (Google, Facebook)
- [ ] Implementaci�n de 2FA
- [ ] Dashboard de m�tricas
- [ ] Reportes y exportaci�n
- [ ] Deploy a producci�n

---

## ?? Contribuci�n

### Clonar y Configurar para Desarrollo

```bash
# Clonar
git clone https://github.com/BenjaminSM04/Sistema-Experto-ONG-Juventud-Sin-Limites.git
cd Sistema-Experto-ONG-Juventud-Sin-Limites

# Restaurar
dotnet restore

# Aplicar migraciones
dotnet ef database update --context ApplicationDbContext

# Ejecutar
dotnet run
```

### Convenciones de C�digo

- Usar **PascalCase** para clases y m�todos
- Usar **camelCase** para variables locales
- Usar **_camelCase** para campos privados
- Agregar comentarios XML en m�todos p�blicos
- Seguir principios SOLID
- Mantener separaci�n de concerns (Domain/Infrastructure)

---

## ?? Licencia

Este proyecto est� bajo licencia privada para uso exclusivo de la ONG Juventud Sin L�mites.

---

## ?? Contacto y Soporte

Para dudas o soporte, consulta la documentaci�n en:
- `IDENTITY_INTEGRATION.md`
- `DATABASE_CODE_FIRST_README.md`
- O abre un issue en GitHub

---

## ?? Notas Importantes

### Seguridad
1. **Cambiar credenciales por defecto** antes de producci�n
2. **Configurar SMTP real** para env�o de emails
3. **Habilitar HTTPS** en producci�n
4. **Configurar CORS** apropiadamente
5. **Usar User Secrets** para datos sensibles en desarrollo

### Performance
1. Los query filters agregan overhead m�nimo
2. Considera �ndices adicionales seg�n uso real
3. El sistema EAV puede ser lento con muchos valores
4. Implementar caching para datos frecuentes

### Backup
1. **Siempre hacer backup** antes de migraciones en producci�n
2. Probar migraciones en ambiente de desarrollo primero
3. Mantener backups regulares de la base de datos

---

## ?? Agradecimientos

Proyecto desarrollado con ?? para la ONG Juventud Sin L�mites

**Versi�n:** 1.0.0  
**�ltima actualizaci�n:** 2024-10-24  
**Framework:** .NET 8  
**Base de Datos:** SQL Server  
**ORM:** Entity Framework Core 8.0.21

---

**�Happy Coding! ??**
