using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sistema_Experto_ONG_Juventud_Sin_Limites;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Api.Models;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;
using Xunit;

namespace Sistema_Experto_ONG.Tests.Api;

public class MotorApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MotorApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
     {
       builder.ConfigureServices(services =>
      {
      // Remover ApplicationDbContext existente
var descriptor = services.SingleOrDefault(
    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
if (descriptor != null)
    services.Remove(descriptor);

             // Agregar InMemory database para pruebas
  services.AddDbContext<ApplicationDbContext>(options =>
       {
   options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
    });

     // Configurar scope para seed
     var sp = services.BuildServiceProvider();
    using var scope = sp.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
           SeedTestData(context);
     });
    });

        _client = _factory.CreateClient();
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Programa
        var programa = new Programa
    {
       Clave = "TEST",
       Nombre = "Programa Test",
  Estado = EstadoGeneral.Activo,
            InferenciaActiva = true,
         CreadoEn = DateTime.UtcNow
 };
  context.Programas.Add(programa);
context.SaveChanges();

        // Regla de prueba
   var regla = new Regla
   {
     Clave = "TEST_REGLA",
       Nombre = "Regla Test",
     Descripcion = "Test",
    Severidad = Severidad.Info,
   Objetivo = ObjetivoRegla.Programa,
   Activa = true,
      Prioridad = 50,
            Version = 1,
    CreadoEn = DateTime.UtcNow
    };
        context.Reglas.Add(regla);
   context.SaveChanges();

        // Alerta de prueba
        var alerta = new Alerta
   {
            ReglaId = regla.ReglaId,
Severidad = Severidad.Alta,
Mensaje = "Alerta de prueba",
          ProgramaId = programa.ProgramaId,
            GeneradaEn = DateTime.UtcNow,
       Estado = EstadoAlerta.Abierta
};
        context.Alertas.Add(alerta);
        context.SaveChanges();
 }

    [Fact]
 public async Task GET_Alertas_SinAutenticacion_DebeRetornar401()
    {
        // Act
      var response = await _client.GetAsync("/api/alertas");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

[Fact]
    public async Task POST_Motor_Ejecutar_SinAutenticacion_DebeRetornar401()
  {
        // Arrange
        var request = new MotorRunDto(DateTime.Now, null);

   // Act
        var response = await _client.PostAsJsonAsync("/api/motor/ejecutar", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // NOTA: Para probar endpoints autenticados, necesitarás implementar
    // autenticación de prueba o usar un TestServer con autenticación configurada.
    // Ejemplo básico de cómo estructurar pruebas autenticadas:

 /*
    [Fact]
    public async Task GET_Alertas_ConAutenticacion_DebeRetornarListaDeAlertas()
    {
      // Arrange
        var client = await GetAuthenticatedClient("admin@ong.com", "Administrador");

 // Act
        var response = await client.GetAsync("/api/alertas");

        // Assert
   Assert.Equal(HttpStatusCode.OK, response.StatusCode);
   var alertas = await response.Content.ReadFromJsonAsync<List<AlertaDto>>();
      Assert.NotNull(alertas);
    }

    [Fact]
    public async Task POST_Motor_Ejecutar_ComoAdministrador_DebeRetornarResumen()
    {
        // Arrange
        var client = await GetAuthenticatedClient("admin@ong.com", "Administrador");
        var request = new MotorRunDto(DateTime.Now, null);

        // Act
        var response = await client.PostAsJsonAsync("/api/motor/ejecutar", request);

    // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<MotorRunResponseDto>();
        Assert.NotNull(resultado);
     Assert.NotNull(resultado.Resumen);
    }

    [Fact]
    public async Task PATCH_Alerta_Estado_ComoCoordinador_DebeActualizarEstado()
    {
 // Arrange
   var client = await GetAuthenticatedClient("coordinador@ong.com", "Coordinador");
        var alertaId = 1; // ID de la alerta seeded
        var request = new AlertaCambioEstadoDto(2, "Resuelto", null); // Estado Resuelta

 // Act
        var response = await client.PatchAsJsonAsync($"/api/alertas/{alertaId}/estado", request);

  // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_Alerta_Estado_ConRowVersionIncorrecto_DebeRetornar409()
    {
     // Arrange
        var client = await GetAuthenticatedClient("admin@ong.com", "Administrador");
        var alertaId = 1;
        var request = new AlertaCambioEstadoDto(2, "Test", new byte[] { 1, 2, 3 }); // RowVersion inválido

        // Act
      var response = await client.PatchAsJsonAsync($"/api/alertas/{alertaId}/estado", request);

        // Assert
 Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<HttpClient> GetAuthenticatedClient(string email, string role)
    {
        // Implementar lógica de autenticación para pruebas
        // Esto requeriría configurar un TestServer con autenticación mock
        throw new NotImplementedException("Implementar autenticación de prueba");
    }
    */
}
