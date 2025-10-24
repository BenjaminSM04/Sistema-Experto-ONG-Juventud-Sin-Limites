# ? Checklist de Verificación - Identity Integration

## ?? Verificación Rápida

Ejecuta estos pasos para verificar que todo funciona correctamente:

---

## 1?? Verificar Base de Datos

### Comando
```bash
dotnet ef dbcontext info --context ApplicationDbContext
```

### Resultado Esperado
```
Type: Sistema_Experto_ONG_Juventud_Sin_Limites.Data.ApplicationDbContext
Provider name: Microsoft.EntityFrameworkCore.SqlServer
Database name: POA_JuventudSinLimites
Data source: (localdb)\mssqllocaldb
```

- [ ] Base de datos existe ?
- [ ] Nombre correcto ?
- [ ] Provider correcto ?

---

## 2?? Verificar Tablas Creadas

### Ver Todas las Tablas
```sql
USE POA_JuventudSinLimites;
GO

SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    CASE 
        WHEN TABLE_NAME LIKE 'Usuario%' OR TABLE_NAME LIKE 'Rol%' THEN 'Identity'
      WHEN TABLE_NAME LIKE 'POA_%' THEN 'POA'
        WHEN TABLE_NAME IN ('Persona', 'Programa', 'UsuarioPrograma') THEN 'Core'
    WHEN TABLE_NAME IN ('Participante', 'Actividad', 'ActividadParticipante', 'Asistencia', 'EvidenciaActividad') THEN 'Operacion'
        WHEN TABLE_NAME IN ('Regla', 'ReglaParametro', 'Alerta', 'RiesgoParticipantePrograma', 'RiesgoDetalle', 'EjecucionMotor', 'MatchRegla', 'DiccionarioObservaciones', 'DiccionarioObservacionesPrograma', 'ReglaParametroOverride') THEN 'Motor'
        WHEN TABLE_NAME IN ('ConfiguracionMotor', 'ConfiguracionMotorOverride') THEN 'Config'
        WHEN TABLE_NAME = 'MetricasProgramaMes' THEN 'BI'
        WHEN TABLE_NAME = 'Logs' THEN 'Audit'
        ELSE 'Other'
    END AS Modulo
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
    AND TABLE_NAME != '__EFMigrationsHistory'
ORDER BY Modulo, TABLE_NAME;
```

### Tablas Esperadas (54 tablas)

#### Identity (7)
- [ ] Usuario
- [ ] Rol
- [ ] UsuarioRol
- [ ] UsuarioClaim
- [ ] UsuarioLogin
- [ ] RolClaim
- [ ] UsuarioToken

#### Core (3)
- [ ] Persona
- [ ] Programa
- [ ] UsuarioPrograma

#### Operacion (5)
- [ ] Participante
- [ ] Actividad
- [ ] ActividadParticipante
- [ ] Asistencia
- [ ] EvidenciaActividad

#### POA (9)
- [ ] POA_Plantilla
- [ ] POA_PlantillaSeccion
- [ ] POA_Campo
- [ ] POA_CampoOpcion
- [ ] POA_CampoValidacion
- [ ] POA_Instancia
- [ ] POA_Valor
- [ ] POA_Archivo
- [ ] POA_SnapshotMensual

#### Motor (10)
- [ ] Regla
- [ ] ReglaParametro
- [ ] ReglaParametroOverride
- [ ] Alerta
- [ ] RiesgoParticipantePrograma
- [ ] RiesgoDetalle
- [ ] EjecucionMotor
- [ ] MatchRegla
- [ ] DiccionarioObservaciones
- [ ] DiccionarioObservacionesPrograma

#### Config (2)
- [ ] ConfiguracionMotor
- [ ] ConfiguracionMotorOverride

#### BI (1)
- [ ] MetricasProgramaMes

#### Audit (1)
- [ ] Logs

---

## 3?? Ejecutar la Aplicación y Verificar Seeder

### Comando
```bash
dotnet run
```

### Verificar en la consola
Deberías ver mensajes como:
```
info: Aplicando migraciones de base de datos...
info: ? Migraciones aplicadas exitosamente
info: Ejecutando seeder de datos iniciales...
info: ? Roles creados exitosamente
info: ? Usuario administrador creado (admin@ong.com / Admin@123)
info: ? Database seeded successfully!
info: ? Datos iniciales cargados exitosamente
```

- [ ] Migraciones aplicadas ?
- [ ] Roles creados ?
- [ ] Usuario admin creado ?
- [ ] Programas creados ?
- [ ] Configuraciones creadas ?
- [ ] Reglas creadas ?

---

## 4?? Verificar Datos del Seeder

### Verificar Roles
```sql
USE POA_JuventudSinLimites;
GO

SELECT Id, Name, Descripcion, IsDeleted
FROM Rol
WHERE IsDeleted = 0
ORDER BY Name;
```

**Esperado (4 roles):**
- [ ] Administrador
- [ ] Coordinador
- [ ] Facilitador
- [ ] Visualizador

### Verificar Usuario Admin
```sql
SELECT 
    u.Id,
    u.UserName,
    u.Email,
    u.EmailConfirmed,
    u.Estado,
    p.Nombres,
    p.Apellidos
FROM Usuario u
INNER JOIN Persona p ON u.PersonaId = p.PersonaId
WHERE u.Email = 'admin@ong.com'
  AND u.IsDeleted = 0;
```

**Esperado:**
- [ ] Email: `admin@ong.com`
- [ ] EmailConfirmed: `1` (true)
- [ ] Estado: `1` (Activo)
- [ ] Nombres: "Administrador"
- [ ] Apellidos: "Del Sistema"

### Verificar Rol del Admin
```sql
SELECT 
    u.Email,
r.Name AS RolNombre
FROM Usuario u
INNER JOIN UsuarioRol ur ON u.Id = ur.UserId
INNER JOIN Rol r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@ong.com'
    AND ur.IsDeleted = 0;
```

**Esperado:**
- [ ] Rol: "Administrador"

### Verificar Programas
```sql
SELECT ProgramaId, Clave, Nombre, Estado, InferenciaActiva, IsDeleted
FROM Programa
WHERE IsDeleted = 0
ORDER BY Clave;
```

**Esperado (4 programas):**
- [ ] EDV - Escuelas de Valores
- [ ] ACADEMIA - Academia de Liderazgo
- [ ] JUVENTUD_SEGURA - Juventud Segura
- [ ] BERNABE - Programa Bernabé

### Verificar Configuraciones del Motor
```sql
SELECT Clave, Valor, Descripcion
FROM ConfiguracionMotor
WHERE IsDeleted = 0
ORDER BY Clave;
```

**Esperado (6 configuraciones):**
- [ ] ASISTENCIA_MIN_PORCENTAJE: 75
- [ ] DIAS_ALERTA_INASISTENCIA: 7
- [ ] FRECUENCIA_EJECUCION_HORAS: 24
- [ ] MOTOR_EJECUCION_AUTO: true
- [ ] UMBRAL_RIESGO_BAJO: 30
- [ ] UMBRAL_RIESGO_MEDIO: 60

### Verificar Reglas del Motor
```sql
SELECT Clave, Nombre, Severidad, Objetivo, Activa, Prioridad
FROM Regla
WHERE IsDeleted = 0
ORDER BY Prioridad DESC;
```

**Esperado (5 reglas):**
- [ ] INASISTENCIA_CONSECUTIVA - Severidad: Alta (2)
- [ ] BAJA_ASISTENCIA_GENERAL - Severidad: Alta (2)
- [ ] ACTIVIDAD_SIN_ASISTENTES - Severidad: Info (1)
- [ ] RETRASO_ACTIVIDAD - Severidad: Alta (2)
- [ ] BAJO_CUMPLIMIENTO_POA - Severidad: Crítica (3)

---

## 5?? Probar Login en la Aplicación

### Pasos
1. Ejecuta `dotnet run`
2. Abre el navegador en `https://localhost:5001` o `http://localhost:5000`
3. Navega a `/Account/Login`
4. Ingresa credenciales:
   - **Email:** `admin@ong.com`
- **Password:** `Admin@123`
5. Haz clic en "Log in"

### Verificar
- [ ] Login exitoso ?
- [ ] Redirección correcta ?
- [ ] Usuario autenticado ?
- [ ] Nombre del usuario visible en la UI ?

---

## 6?? Verificar Índices Creados

```sql
USE POA_JuventudSinLimites;
GO

SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.is_unique AS IsUnique,
    i.filter_definition AS FilterDefinition
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name IS NOT NULL
    AND t.name IN ('Usuario', 'Rol', 'UsuarioRol')
ORDER BY t.name, i.name;
```

### Índices Esperados en Usuario
- [ ] `PK_Usuario` (Primary Key)
- [ ] `UserNameIndex` (NormalizedUserName)
- [ ] `EmailIndex` (NormalizedEmail)
- [ ] `IX_Usuario_Email_Unique` (Email WHERE IsDeleted = 0)
- [ ] `IX_Usuario_PersonaId` (PersonaId)

### Índices Esperados en Rol
- [ ] `PK_Rol` (Primary Key)
- [ ] `RoleNameIndex` (NormalizedName)
- [ ] `IX_Rol_Nombre_Unique` (Name WHERE IsDeleted = 0)

### Índices Esperados en UsuarioRol
- [ ] `PK_UsuarioRol` (UserId, RoleId)
- [ ] `IX_UsuarioRol_RoleId` (RoleId)

---

## 7?? Verificar Soft Delete

### Crear un rol de prueba
```sql
INSERT INTO Rol (Name, NormalizedName, Descripcion, CreadoEn, IsDeleted)
VALUES ('TestRol', 'TESTROL', 'Rol de prueba', GETUTCDATE(), 0);
```

### Verificar que existe
```sql
SELECT * FROM Rol WHERE Name = 'TestRol' AND IsDeleted = 0;
```
- [ ] Rol visible ?

### Hacer soft delete
```sql
UPDATE Rol 
SET IsDeleted = 1, 
  EliminadoEn = GETUTCDATE()
WHERE Name = 'TestRol';
```

### Verificar que no aparece en consulta normal
```sql
SELECT * FROM Rol WHERE Name = 'TestRol' AND IsDeleted = 0;
```
- [ ] No devuelve resultados ?

### Verificar que sigue existiendo (sin filtro)
```sql
SELECT * FROM Rol WHERE Name = 'TestRol';
```
- [ ] Rol todavía existe con IsDeleted = 1 ?

### Limpiar
```sql
DELETE FROM Rol WHERE Name = 'TestRol';
```

---

## 8?? Verificar Auditoría Automática

### Crear una persona
```sql
INSERT INTO Persona (Nombres, Apellidos, FechaNacimiento, Telefono, CreadoEn, IsDeleted)
VALUES ('Test', 'User', '1990-01-01', '1234-5678', GETUTCDATE(), 0);
```

### Verificar campo CreadoEn
```sql
SELECT PersonaId, Nombres, Apellidos, CreadoEn, ActualizadoEn
FROM Persona
WHERE Nombres = 'Test';
```
- [ ] CreadoEn tiene valor ?
- [ ] ActualizadoEn es NULL ?

### Actualizar
```sql
UPDATE Persona 
SET Telefono = '8765-4321'
WHERE Nombres = 'Test';
```

**Nota:** El interceptor solo funciona desde el código C#, no desde SQL directo.

### Limpiar
```sql
DELETE FROM Persona WHERE Nombres = 'Test';
```

---

## 9?? Probar Creación de Usuario con UserManager (En Blazor)

Crea un componente de prueba:

```razor
@page "/test-user-creation"
@inject UserManager<Usuario> UserManager
@inject ApplicationDbContext Context

<h3>Test: Crear Usuario</h3>

<button @onclick="CreateTestUser">Crear Usuario de Prueba</button>

@if (!string.IsNullOrEmpty(message))
{
    <div class="alert alert-info mt-3">@message</div>
}

@code {
    private string message = "";

    private async Task CreateTestUser()
    {
        try
        {
         // Crear persona
         var persona = new Persona
            {
      Nombres = "Juan",
  Apellidos = "Pérez",
 FechaNacimiento = new DateTime(1995, 5, 15),
         Telefono = "1234-5678"
            };
  Context.Personas.Add(persona);
  await Context.SaveChangesAsync();

    // Crear usuario
         var usuario = new Usuario
       {
                PersonaId = persona.PersonaId,
   UserName = "juan.perez@test.com",
  Email = "juan.perez@test.com",
             EmailConfirmed = true,
       Estado = EstadoGeneral.Activo
     };

            var result = await UserManager.CreateAsync(usuario, "TestUser@123");

      if (result.Succeeded)
{
          await UserManager.AddToRoleAsync(usuario, "Facilitador");
          message = "? Usuario creado exitosamente!";
   }
      else
    {
         message = "? Errores: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
        }
        catch (Exception ex)
        {
      message = "? Excepción: " + ex.Message;
        }
    }
}
```

- [ ] Usuario se crea correctamente ?
- [ ] Password se hashea automáticamente ?
- [ ] Rol se asigna correctamente ?

---

## ?? Compilación y Ejecución

### Compilar
```bash
dotnet build
```

**Resultado Esperado:**
```
Build succeeded.
    0 Warning(s)
 0 Error(s)
```

- [ ] Compilación exitosa ?
- [ ] Sin errores ?
- [ ] Sin warnings ?

### Ejecutar
```bash
dotnet run
```

**Resultado Esperado:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

- [ ] Aplicación se ejecuta ?
- [ ] Seeder ejecutado ?
- [ ] Sin errores en consola ?

---

## ? Resumen del Checklist

### Base de Datos
- [ ] Base de datos creada
- [ ] 54 tablas creadas
- [ ] Índices correctos
- [ ] Soft delete funciona

### Datos Iniciales (Seeder)
- [ ] 4 Roles creados
- [ ] Usuario admin creado
- [ ] 4 Programas creados
- [ ] 6 Configuraciones creadas
- [ ] 5 Reglas creadas

### Funcionalidad
- [ ] Login funciona
- [ ] UserManager funciona
- [ ] RoleManager funciona
- [ ] Auditoría automática
- [ ] Soft delete automático

### Código
- [ ] Compilación exitosa
- [ ] Sin errores
- [ ] Aplicación se ejecuta

---

## ?? Si todos los checks están ?

**¡FELICITACIONES!** 

La integración de Identity está **100% funcional** y lista para desarrollo.

Puedes comenzar a:
1. Crear interfaces de usuario personalizadas
2. Implementar lógica de negocio
3. Agregar más funcionalidades
4. Personalizar el sistema de autenticación

---

**Fecha de Verificación:** ________________  
**Verificado por:** ________________  
**Estado:** [ ] ? TODO OK  |  [ ] ?? REVISAR  |  [ ] ? ERROR

