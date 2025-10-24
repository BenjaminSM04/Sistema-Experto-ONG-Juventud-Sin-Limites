# ??? Comandos �tiles - Sistema Experto ONG

## ?? Entity Framework Core

### Migraciones

```powershell
# Ver todas las migraciones
dotnet ef migrations list --context ApplicationDbContext

# Crear nueva migraci�n
dotnet ef migrations add NombreDeLaMigracion --context ApplicationDbContext --output-dir Data/Migrations

# Aplicar migraciones a la base de datos
dotnet ef database update --context ApplicationDbContext

# Volver a una migraci�n espec�fica
dotnet ef database update NombreDeLaMigracion --context ApplicationDbContext

# Remover �ltima migraci�n (si no se aplic�)
dotnet ef migrations remove --context ApplicationDbContext

# Generar script SQL de todas las migraciones
dotnet ef migrations script --context ApplicationDbContext --output migration.sql

# Generar script SQL incremental (de una migraci�n a otra)
dotnet ef migrations script MigracionInicial MigracionFinal --context ApplicationDbContext --output incremental.sql

# Ver SQL que se ejecutar� sin aplicarlo
dotnet ef migrations script --context ApplicationDbContext --idempotent
```

### Base de Datos

```powershell
# Eliminar la base de datos
dotnet ef database drop --context ApplicationDbContext

# Crear/actualizar base de datos y aplicar migraciones
dotnet ef database update --context ApplicationDbContext

# Ver informaci�n de la base de datos
dotnet ef dbcontext info --context ApplicationDbContext

# Generar diagrama del modelo (requiere extensi�n)
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Models
```

## ??? Compilaci�n y Ejecuci�n

```powershell
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Compilar en Release
dotnet build --configuration Release

# Limpiar
dotnet clean

# Ejecutar la aplicaci�n
dotnet run

# Ejecutar en modo watch (recarga autom�tica)
dotnet watch run

# Ejecutar con un perfil espec�fico
dotnet run --launch-profile "https"
```

## ?? Testing (cuando agregues tests)

```powershell
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con detalles
dotnet test --verbosity detailed

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests espec�ficos
dotnet test --filter "FullyQualifiedName~Namespace.TestClass"
```

## ?? Gesti�n de Paquetes

```powershell
# Agregar paquete NuGet
dotnet add package NombreDelPaquete

# Agregar paquete con versi�n espec�fica
dotnet add package NombreDelPaquete --version 1.0.0

# Remover paquete
dotnet remove package NombreDelPaquete

# Listar paquetes instalados
dotnet list package

# Listar paquetes obsoletos
dotnet list package --outdated

# Actualizar paquetes
dotnet add package NombreDelPaquete
```

## ?? Inspecci�n del Proyecto

```powershell
# Ver informaci�n del proyecto
dotnet --info

# Listar SDKs instalados
dotnet --list-sdks

# Listar runtimes instalados
dotnet --list-runtimes

# Ver estructura del proyecto
tree /F /A
```

## ??? SQL Server Espec�fico

```powershell
# Conectar a SQL Server con sqlcmd
sqlcmd -S (localdb)\mssqllocaldb -d POA_JuventudSinLimites

# Backup de la base de datos
sqlcmd -S (localdb)\mssqllocaldb -Q "BACKUP DATABASE [POA_JuventudSinLimites] TO DISK='C:\Backup\POA.bak'"

# Restaurar base de datos
sqlcmd -S (localdb)\mssqllocaldb -Q "RESTORE DATABASE [POA_JuventudSinLimites] FROM DISK='C:\Backup\POA.bak'"
```

## ?? Debugging

```powershell
# Ver logs de EF Core
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Ver SQL generado en consola (agregar en appsettings.Development.json):
{
  "Logging": {
    "LogLevel": {
 "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## ?? Publicaci�n

```powershell
# Publicar para producci�n
dotnet publish --configuration Release --output ./publish

# Publicar autosuficiente (incluye runtime)
dotnet publish --configuration Release --self-contained true --runtime win-x64 --output ./publish

# Publicar con archivo �nico
dotnet publish --configuration Release --runtime win-x64 --output ./publish /p:PublishSingleFile=true

# Publicar sin dependencias del runtime
dotnet publish --configuration Release --no-self-contained --runtime win-x64 --output ./publish
```

## ?? Scripts �tiles

### Resetear Base de Datos (Development)
```powershell
# reset-database.ps1
dotnet ef database drop --context ApplicationDbContext --force
dotnet ef database update --context ApplicationDbContext
Write-Host "? Base de datos reseteada" -ForegroundColor Green
```

### Crear Migraci�n con Timestamp
```powershell
# new-migration.ps1
param([string]$name)
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$migrationName = "$timestamp" + "_$name"
dotnet ef migrations add $migrationName --context ApplicationDbContext --output-dir Data/Migrations
Write-Host "? Migraci�n creada: $migrationName" -ForegroundColor Green
```

### Backup Autom�tico
```powershell
# backup-database.ps1
$date = Get-Date -Format "yyyy-MM-dd_HHmmss"
$backupPath = "C:\Backups\POA_$date.bak"
sqlcmd -S (localdb)\mssqllocaldb -Q "BACKUP DATABASE [POA_JuventudSinLimites] TO DISK='$backupPath'"
Write-Host "? Backup creado: $backupPath" -ForegroundColor Green
```

### Ver Estad�sticas del C�digo
```powershell
# stats.ps1
Write-Host "?? Estad�sticas del Proyecto" -ForegroundColor Cyan
Write-Host "Entidades Domain:" (Get-ChildItem -Path "Domain" -Recurse -Filter "*.cs" | Measure-Object).Count
Write-Host "Configuraciones:" (Get-ChildItem -Path "Infrastructure/Configurations" -Recurse -Filter "*.cs" | Measure-Object).Count
Write-Host "Total archivos .cs:" (Get-ChildItem -Recurse -Filter "*.cs" | Measure-Object).Count
Write-Host "L�neas de c�digo:" ((Get-ChildItem -Recurse -Filter "*.cs" | Get-Content | Measure-Object -Line).Lines)
```

## ?? Configuraci�n �til en appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=POA_JuventudSinLimites;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
"LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## ?? Visual Studio Code - Tasks

Crear `.vscode/tasks.json`:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
 "label": "build",
  "command": "dotnet",
      "type": "process",
      "args": ["build"],
      "problemMatcher": "$msCompile"
    },
    {
   "label": "migrate",
"command": "dotnet",
      "type": "process",
      "args": ["ef", "database", "update", "--context", "ApplicationDbContext"]
    },
  {
      "label": "new-migration",
      "command": "dotnet",
 "type": "process",
      "args": ["ef", "migrations", "add", "${input:migrationName}", "--context", "ApplicationDbContext"]
    }
  ],
  "inputs": [
    {
      "id": "migrationName",
      "type": "promptString",
      "description": "Nombre de la migraci�n"
    }
  ]
}
```

## ?? Atajos de Teclado (VS Code)

- `Ctrl + Shift + B` - Build
- `F5` - Run with debugging
- `Ctrl + F5` - Run without debugging
- `Ctrl + Shift + P` ? "Tasks: Run Task" - Ejecutar tareas personalizadas

## ?? Enlaces �tiles

- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
- [Migrations Overview](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [ASP.NET Core Blazor](https://docs.microsoft.com/aspnet/core/blazor/)
- [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)

---

**?? Tip**: Guarda estos comandos en favoritos o cr�ate scripts PowerShell para los que uses m�s frecuentemente.
