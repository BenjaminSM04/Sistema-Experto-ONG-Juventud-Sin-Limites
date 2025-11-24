using Microsoft.Extensions.Hosting;
namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference

{
    public class MotorScheduler : BackgroundService
    {
        private readonly IServiceProvider _sp;
        public MotorScheduler(IServiceProvider sp) => _sp = sp;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var nowLocal = DateTime.Now; // puedes leer hora desde ConfiguracionMotor
                if (nowLocal.Hour == 2) // ejemplo: 02:00 local
                {
                    using var scope = _sp.CreateScope();
                    var motor = scope.ServiceProvider.GetRequiredService<IMotorInferencia>();
                    await motor.EjecutarAsync(DateOnly.FromDateTime(DateTime.UtcNow), null, false, stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(61), stoppingToken); // evita doble corrida en la misma hora
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
