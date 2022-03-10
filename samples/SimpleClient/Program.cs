using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = HostingHostBuilderExtensions.ConfigureLogging(new HostBuilder(), logging =>
            {
                logging.AddConsole();
                //Microsoft.Extensions.Logging.LoggingBuilderExtensions.SetMinimumLevel(logging, (LogLevel)LogLevel.Debug);
            })
            .ConfigureServices(services =>
            {
                services.AddMqttFxClient(options =>
                {
                    options.Host = "broker.emqx.io";
                    options.Port = 1883;
                    options.WillTopic = "testtopic/c";
                    options.WillPayload = Encoding.UTF8.GetBytes("offline");
                    options.WillRetain = true;
                });
                services.AddHostedService<Services>();
            });
            await host.RunConsoleAsync();
        }
    }
}
