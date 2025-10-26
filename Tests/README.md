# 🧪 Tests del Sistema Experto ONG

## ⚠️ IMPORTANTE

**Los tests NO se ejecutan automáticamente al iniciar la aplicación.**

Solo se ejecutan cuando los corres manualmente usando:
- Test Explorer en Visual Studio
- Comando `dotnet test` en terminal

---

## 📁 Estructura de Tests

```
Tests/
├── Sistema_Experto_ONG.Tests.csproj
├── UnitTest1.cs (Tests de ejemplo)
├── Infrastructure/
│   └── Services/
│       ├── MotorInferenciaTests.cs
│       └── FeatureProviderTests.cs
└── Api/
    └── MotorApiTests.cs
```

---

## 🚀 Ejecutar Tests

### Desde Visual Studio

1. **Abrir Test Explorer**
   - Menú: `Ver → Test Explorer`
   - Atajo: `Ctrl + E, T`

2. **Ejecutar Tests**
   - Todos los tests: Click en "Run All"
   - Test específico: Click derecho → "Run"
   - Con debug: Click derecho → "Debug"

### Desde Terminal

```bash
# Navegar a la carpeta del proyecto
cd E:\Benja\source\repos\Sistema-Experto-ONG-Juventud-Sin-Limites

# Ejecutar todos los tests
dotnet test

# Ejecutar tests de un proyecto específico
dotnet test Tests/Sistema_Experto_ONG.Tests.csproj

# Ejecutar tests con filtro
dotnet test --filter "FullyQualifiedName~MotorInferenciaTests"

# Ejecutar con output detallado
dotnet test --logger "console;verbosity=detailed"

# Ejecutar con cobertura de código
dotnet test /p:CollectCoverage=true
```

---

## 📝 Escribir Nuevos Tests

### Test Simple

```csharp
using Xunit;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Tests;

public class MiTest
{
    [Fact]
    public void Test_Suma_Correcta()
    {
        // Arrange
        int a = 2;
        int b = 3;
        int expected = 5;

   // Act
        int actual = a + b;

        // Assert
        Assert.Equal(expected, actual);
    }
}
```

### Test con Parámetros (Theory)

```csharp
[Theory]
[InlineData(1, 1, 2)]
[InlineData(2, 2, 4)]
[InlineData(5, 5, 10)]
public void Test_Suma_MultiplesValores(int a, int b, int expected)
{
    // Act
    var actual = a + b;

    // Assert
    Assert.Equal(expected, actual);
}
```

### Test Asíncrono

```csharp
[Fact]
public async Task Test_Operacion_Asincrona()
{
    // Arrange
    var service = new MiServicio();

    // Act
    var resultado = await service.ObtenerDatosAsync();

    // Assert
  Assert.NotNull(resultado);
}
```

### Test con Base de Datos In-Memory

```csharp
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;

public class DatabaseTest
{
    private ApplicationDbContext CreateContext()
    {
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
   .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Test_Puede_Guardar_Programa()
    {
        // Arrange
  await using var context = CreateContext();
   var programa = new Programa
        {
 Clave = "TEST",
            Nombre = "Programa de Test",
  Estado = EstadoGeneral.Activo
        };

        // Act
        context.Programas.Add(programa);
        await context.SaveChangesAsync();

        // Assert
        var guardado = await context.Programas.FirstOrDefaultAsync();
 Assert.NotNull(guardado);
     Assert.Equal("TEST", guardado.Clave);
    }
}
```

---

## 🔧 Configuración del Proyecto

### archivo `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
 <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />
  </ItemGroup>

  <ItemGroup>
 <ProjectReference Include="..\Sistema-Experto-ONG-Juventud-Sin-Limites.csproj" />
  </ItemGroup>
</Project>
```

---

## 📊 Assertions Comunes

### Igualdad

```csharp
Assert.Equal(expected, actual);
Assert.NotEqual(expected, actual);
```

### Booleanos

```csharp
Assert.True(condition);
Assert.False(condition);
```

### Nulos

```csharp
Assert.Null(obj);
Assert.NotNull(obj);
```

### Colecciones

```csharp
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
```

### Excepciones

```csharp
Assert.Throws<InvalidOperationException>(() => metodo());
await Assert.ThrowsAsync<ArgumentException>(async () => await metodAsync());
```

### Strings

```csharp
Assert.Contains("substring", texto);
Assert.StartsWith("prefix", texto);
Assert.EndsWith("suffix", texto);
Assert.Matches("regex", texto);
```

---

## 🎯 Buenas Prácticas

1. **Nombrar tests claramente**
   - `Test_Metodo_Escenario_ResultadoEsperado`
   - `Test_GuardarPrograma_ConDatosValidos_RetornaExito`

2. **Usar patrón AAA (Arrange-Act-Assert)**
   ```csharp
   // Arrange - Preparar datos y objetos
   // Act - Ejecutar la acción a probar
 // Assert - Verificar el resultado
   ```

3. **Un assert por test** (preferible)
   - Cada test debe verificar una sola cosa

4. **Tests independientes**
   - No deben depender del orden de ejecución
   - Usar bases de datos in-memory con nombres únicos

5. **Limpiar recursos**
   - Usar `await using` para DbContext
 - Implementar `IDisposable` si es necesario

---

## 📚 Recursos

- [Documentación xUnit](https://xunit.net/)
- [EF Core In-Memory Database](https://learn.microsoft.com/ef/core/testing/testing-with-the-in-memory-database)
- [Best Practices for Testing](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices)

---

**Última actualización**: 2025-01-26
