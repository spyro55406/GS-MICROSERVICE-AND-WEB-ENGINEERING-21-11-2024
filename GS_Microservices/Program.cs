using GS_Microservices.Services;

namespace GS_Microservices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        // Configuração do MongoDB
                        services.AddSingleton<EnergyService>();

                        // Configuração do Redis
                        var redisSettings = context.Configuration.GetSection("Redis").Get<RedisSettings>();

                        if (string.IsNullOrEmpty(redisSettings?.ConnectionString))
                        {
                            throw new ArgumentNullException("Redis ConnectionString", "A string de conexão do Redis não foi configurada.");
                        }

                        services.AddSingleton(redisSettings);
                        services.AddSingleton<RedisCacheService>();

                        // Adiciona os controladores (API)
                        services.AddControllers();

                        // Adiciona o Swagger para documentação da API
                        services.AddEndpointsApiExplorer();
                        services.AddSwaggerGen();
                    });

                    webBuilder.Configure(app =>
                    {
                        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

                        if (env.IsDevelopment())
                        {
                            app.UseSwagger();
                            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energy Monitoring API v1"));
                        }

                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                });
    }
}
