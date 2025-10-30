-- Script para agregar campos de gestión de contraseñas a la tabla Usuario
-- Ejecutar este script manualmente en SQL Server Management Studio o similar

USE POA_JuventudSinLimites;
GO

-- Agregar columnas para gestión de contraseñas obligatorias
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') AND name = 'MustChangePassword')
BEGIN
  ALTER TABLE [dbo].[Usuario]
 ADD [MustChangePassword] bit NOT NULL DEFAULT 1;
    PRINT 'Columna MustChangePassword agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Columna MustChangePassword ya existe.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[Usuario]
    ADD [CreatedBy] nvarchar(256) NULL;
    PRINT 'Columna CreatedBy agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Columna CreatedBy ya existe.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') AND name = 'CreatedAtUtc')
BEGIN
    ALTER TABLE [dbo].[Usuario]
    ADD [CreatedAtUtc] datetime2(0) NOT NULL DEFAULT (GETUTCDATE());
    PRINT 'Columna CreatedAtUtc agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Columna CreatedAtUtc ya existe.';
END
GO

-- Actualizar usuario admin existente para que NO necesite cambiar contraseña
UPDATE [dbo].[Usuario]
SET [MustChangePassword] = 0,
    [CreatedBy] = 'Sistema',
    [CreatedAtUtc] = GETUTCDATE()
WHERE [Email] = 'admin@ong.com';

PRINT 'Usuario admin actualizado: MustChangePassword = false';
GO

-- Verificar cambios
SELECT 
    [Id],
    [Email],
    [MustChangePassword],
    [CreatedBy],
    [CreatedAtUtc]
FROM [dbo].[Usuario]
WHERE [IsDeleted] = 0;
GO

PRINT 'Script completado exitosamente.';
