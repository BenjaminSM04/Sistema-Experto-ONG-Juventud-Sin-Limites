using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private const string API_KEY = "595d6461dbeec4d2b3a5741d04522341";
    private const string BASE_URL = "https://api.openweathermap.org/data/2.5/weather";

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherData?> ObtenerClimaAsync(string ciudad = "La Paz", string pais = "BO")
    {
        try
        {
            var url = $"{BASE_URL}?q={ciudad},{pais}&appid={API_KEY}&units=metric&lang=es";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al obtener clima: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var weatherResponse = JsonSerializer.Deserialize<OpenWeatherResponse>(json);

            if (weatherResponse == null)
                return null;

            return new WeatherData
            {
                Temperatura = Math.Round(weatherResponse.Main.Temp),
                Descripcion = CapitalizarPrimeraLetra(weatherResponse.Weather[0].Description),
                IconoCodigo = weatherResponse.Weather[0].Icon,
                Ciudad = weatherResponse.Name,
                Humedad = weatherResponse.Main.Humidity,
                SensacionTermica = Math.Round(weatherResponse.Main.FeelsLike),
                VelocidadViento = Math.Round(weatherResponse.Wind.Speed * 3.6, 1) // Convertir m/s a km/h
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos del clima");
            return null;
        }
    }

    private string CapitalizarPrimeraLetra(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        return char.ToUpper(texto[0]) + texto.Substring(1);
    }
}

public class WeatherData
{
    public double Temperatura { get; set; }
    public string Descripcion { get; set; } = "";
    public string IconoCodigo { get; set; } = "";
    public string Ciudad { get; set; } = "";
    public int Humedad { get; set; }
    public double SensacionTermica { get; set; }
    public double VelocidadViento { get; set; }

    public string IconoUrl => $"https://openweathermap.org/img/wn/{IconoCodigo}@2x.png";
}

// Clases para deserializar la respuesta de OpenWeather API
internal class OpenWeatherResponse
{
    [JsonPropertyName("main")]
    public MainData Main { get; set; } = new();

    [JsonPropertyName("weather")]
    public List<WeatherInfo> Weather { get; set; } = new();

    [JsonPropertyName("wind")]
    public WindData Wind { get; set; } = new();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

internal class MainData
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

internal class WeatherInfo
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "";
}

internal class WindData
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}
