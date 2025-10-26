# 👨‍💻 Guía para Desarrolladores

## 📋 Índice

- [Arquitectura del Proyecto](#arquitectura-del-proyecto)
- [Configuración del Entorno](#configuración-del-entorno)
- [Flujo de Trabajo](#flujo-de-trabajo)
- [Convenciones de Código](#convenciones-de-código)
- [Testing](#testing)
- [Debugging](#debugging)

---

## 🏗️ Arquitectura del Proyecto

### Estructura de Carpetas

```
Sistema-Experto-ONG-Juventud-Sin-Limites/
├── Components/         # Componentes Blazor
│   ├── Account/   # Autenticación
│   ├── Layout/      # Layouts
│   └── Pages/         # Páginas
├── Domain/            # Capa de Dominio
├── Infrastructure/    # Implementaciones
├── Data/   # DbContext
├── Api/     # DTOs y modelos API
└── Tests/        # Pruebas unitarias
```

### Patrones y Principios

- **Domain-Driven Design (DDD)**: Separación clara entre dominio e infraestructura
- **Repository Pattern**: Acceso a datos a través de DbContext
- **Dependency Injection**: Servicios registrados en Program.cs
- **Code First**: Migraciones de EF Core para gestión de esquema

---

## ⚙️ Configuración del Entorno

### Prerrequisitos

- Visual Studio 2022 o VS Code
- .NET 8 SDK
- SQL Server LocalDB o SQL Server

### Comandos Esenciales de EF Core

```bash
# Ver migraciones
dotnet ef migrations list --context ApplicationDbContext

# Crear migración
dotnet ef migrations add NombreMigracion --context ApplicationDbContext

# Aplicar migraciones
dotnet ef database update --context ApplicationDbContext

# Eliminar última migración
dotnet ef migrations remove --context ApplicationDbContext

# Generar script SQL
dotnet ef migrations script --context ApplicationDbContext --output migration.sql
```

### Build y Ejecución

```bash
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Ejecutar con watch (hot reload)
dotnet watch run
```

---

## 🔄 Flujo de Trabajo

### 1. Modificar Entidades

```csharp
// Domain/Operacion/Participante.cs
public class Participante : BaseEntity
{
    // Agregar nueva propiedad
    public string? NumeroDocumento { get; set; }
}
```

### 2. Configurar en Fluent API

```csharp
// Infrastructure/Configurations/Operacion/ParticipanteConfig.cs
public void Configure(EntityTypeBuilder<Participante> builder)
{
    builder.Property(p => p.NumeroDocumento)
      .HasMaxLength(20);
}
```

### 3. Crear y Aplicar Migración

```bash
dotnet ef migrations add AgregarNumeroDocumento --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
```

---

## 📝 Convenciones de Código

### Nomenclatura

- **Clases**: PascalCase (`ParticipanteConfig`)
- **Métodos**: PascalCase (`ObtenerParticipantes`)
- **Variables locales**: camelCase (`participante`, `listaUsuarios`)
- **Campos privados**: _camelCase (`_context`, `_logger`)
- **Constantes**: UPPER_CASE (`MAX_INTENTOS`)

### Ejemplo de Servicio

```csharp
public class ParticipanteService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ParticipanteService> _logger;

    public ParticipanteService(
   ApplicationDbContext context,
      ILogger<ParticipanteService> logger)
    {
        _context = context;
  _logger = logger;
    }

    public async Task<List<Participante>> ObtenerActivosAsync()
    {
 return await _context.Participantes
            .Where(p => p.Estado == EstadoGeneral.Activo)
       .ToListAsync();
    }
}
```

---

## 🧪 Testing

### Estructura de Tests

```
Tests/
├── Infrastructure/
│   └── Services/
│       ├── MotorInferenciaTests.cs
│    └── FeatureProviderTests.cs
└── Api/
    └── MotorApiTests.cs
```

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests específicos
dotnet test --filter "FullyQualifiedName~MotorInferenciaTests"

# Con cobertura
dotnet test /p:CollectCoverage=true
```

---

## 🐛 Debugging

### Logs en Blazor

```csharp
// En componentes Blazor
Console.WriteLine("🎯 Información de debug");

// En servicios
_logger.LogInformation("✅ Operación exitosa");
_logger.LogWarning("⚠️ Advertencia");
_logger.LogError("❌ Error crítico");
```

### Ver Logs en Visual Studio

- **Output Window**: Ver logs del servidor
- **Console del navegador** (F12): Ver logs de Blazor

### Breakpoints

- Usar breakpoints en Visual Studio
- Inspeccionar variables con el debugger
- Watch window para expresiones

---

## 🔑 Accesos por Defecto

### Usuario Administrador

```
Email:    admin@ong.com
Password: Admin@123
```

⚠️ **Cambiar en producción**

---

## 📚 Recursos Adicionales

- [Documentación .NET 8](https://learn.microsoft.com/dotnet)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor)
- [Entity Framework Core](https://learn.microsoft.com/ef/core)
- [MudBlazor Components](https://mudblazor.com)

---

**Última actualización**: 2025-01-26
