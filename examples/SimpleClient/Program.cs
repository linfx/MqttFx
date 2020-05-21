using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SimpleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = HostingHostBuilderExtensions.ConfigureLogging(new HostBuilder()
, logging =>
                 {
                     logging.AddConsole();
                     Microsoft.Extensions.Logging.LoggingBuilderExtensions.SetMinimumLevel(logging, (LogLevel)LogLevel.Debug);
                 })
                .ConfigureServices((System.Action<IServiceCollection>)(services =>
                {
                    MqttFxServiceCollectionExtensions.AddMqttClient(services, (System.Action<MqttFx.MqttClientOptions>)(options =>
                     {
                         options.Host = "127.0.0.1";
                     }));
                    services.AddHostedService<Services>();
                }));
            await host.RunConsoleAsync();
        }
    }
}
