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

### ⚠️ IMPORTANTE: Los tests NO se ejecutan automáticamente

Los tests están configurados para ejecutarse **SOLO cuando tú los ejecutes manualmente**. 
No se ejecutan al iniciar la aplicación con `dotnet run` o F5 en Visual Studio.

### Ejecutar Tests

#### Desde Visual Studio
1. Abre el **Test Explorer** (Ver → Test Explorer)
2. Haz clic derecho en el test que quieres ejecutar
3. Selecciona "Run" o "Debug"

#### Desde la terminal

```bash
# Todos los tests
dotnet test

# Tests de un proyecto específico
dotnet test Tests/Sistema_Experto_ONG.Tests.csproj

# Tests específicos por nombre
dotnet test --filter "FullyQualifiedName~MotorInferenciaTests"

# Tests con cobertura
dotnet test /p:CollectCoverage=true

# Tests con output detallado
dotnet test --logger "console;verbosity=detailed"
```

### Crear Nuevos Tests

```csharp
namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Tests;

public class MiNuevoTest
{
    [Fact]
public void Test_Descripcion_Resultado()
    {
        // Arrange (Preparar)
        var expected = 5;

 // Act (Actuar)
        var actual = 2 + 3;

   // Assert (Verificar)
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 2, 4)]
    [InlineData(3, 3, 6)]
    public void Test_Suma_ConDatos(int a, int b, int expected)
    {
        // Arrange + Act
        var actual = a + b;

    // Assert
        Assert.Equal(expected, actual);
    }
}
```

### Tests con Base de Datos In-Memory

```csharp
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;

public class DatabaseTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Test_Puede_Agregar_Programa()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var programa = new Programa 
        { 
        Clave = "TEST", 
            Nombre = "Test Program" 
        };

        // Act
context.Programas.Add(programa);
        await context.SaveChangesAsync();

        // Assert
        var resultado = await context.Programas.FirstAsync();
   Assert.Equal("TEST", resultado.Clave);
    }
}
```

### Configuración del Proyecto de Tests

El proyecto de tests (`Tests/Sistema_Experto_ONG.Tests.csproj`) tiene:

- `<IsTestProject>true</IsTestProject>` - Marca el proyecto como de tests
- `<IsPackable>false</IsPackable>` - No se empaqueta
- Referencia al proyecto principal
- Paquetes de xUnit y EntityFrameworkCore.InMemory

### Excluir Tests del Proyecto Principal

El proyecto principal tiene una exclusión explícita:

```xml
<ItemGroup>
    <Compile Remove="Tests\**" />
    <Content Remove="Tests\**" />
    <EmbeddedResource Remove="Tests\**" />
    <None Remove="Tests\**" />
</ItemGroup>
```

Esto asegura que los tests **nunca** se ejecuten al correr la aplicación.

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
