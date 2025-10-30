-- Script para verificar el estado de las tablas después del seeding
USE POA_JuventudSinLimites;
GO

PRINT '========================================';
PRINT 'VERIFICACIÓN DE SEEDING';
PRINT '========================================';
PRINT '';

-- 1. Verificar Roles
PRINT '1. ROLES:';
SELECT 
    Id,
    Name,
    Descripcion,
    CreadoEn,
    IsDeleted
FROM [dbo].[Rol]
WHERE IsDeleted = 0;
PRINT '';

-- 2. Verificar Personas
PRINT '2. PERSONAS:';
SELECT 
    PersonaId,
    Nombres,
    Apellidos,
    Telefono,
    CreadoEn,
    IsDeleted
FROM [dbo].[Persona]
WHERE IsDeleted = 0;
PRINT '';

-- 3. Verificar Usuarios
PRINT '3. USUARIOS:';
SELECT 
    Id,
    Email,
    UserName,
    EmailConfirmed,
    Estado,
    MustChangePassword,
    CreatedBy,
    CreatedAtUtc,
    PersonaId,
    CreadoEn,
    IsDeleted
FROM [dbo].[Usuario]
WHERE IsDeleted = 0;
PRINT '';

-- 4. Verificar UsuarioRol
PRINT '4. USUARIO-ROL:';
SELECT 
    ur.UserId,
    ur.RoleId,
    u.Email,
    r.Name as RolNombre
FROM [dbo].[UsuarioRol] ur
INNER JOIN [dbo].[Usuario] u ON ur.UserId = u.Id
INNER JOIN [dbo].[Rol] r ON ur.RoleId = r.Id
WHERE u.IsDeleted = 0;
PRINT '';

-- 5. Verificar Programas
PRINT '5. PROGRAMAS:';
SELECT 
    ProgramaId,
    Clave,
    Nombre,
    Estado,
    IsDeleted
FROM [dbo].[Programa]
WHERE IsDeleted = 0;
PRINT '';

-- 6. Resumen
PRINT '========================================';
PRINT 'RESUMEN:';
PRINT '========================================';
SELECT 
    'Roles' as Tabla,
    COUNT(*) as Cantidad
FROM [dbo].[Rol]
WHERE IsDeleted = 0

UNION ALL

SELECT 
    'Personas',
    COUNT(*)
FROM [dbo].[Persona]
WHERE IsDeleted = 0

UNION ALL

SELECT 
    'Usuarios',
    COUNT(*)
FROM [dbo].[Usuario]
WHERE IsDeleted = 0

UNION ALL

SELECT 
  'Programas',
    COUNT(*)
FROM [dbo].[Programa]
WHERE IsDeleted = 0;
