using Microsoft.Extensions.DependencyInjection;
using nMqtt.Transport.Sockets;
using System;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var services = new ServiceCollection();
            services.AddSockets(options =>
            {
                options.IOQueueCount = 1;
            });
            var container = services.BuildServiceProvider();

            var f = container.GetService<SocketTransportFactory>();
            var transport = f.Create();


            await transport.BindAsync().ConfigureAwait(false);


            Console.ReadKey();
        }



    }

    public static class Ext
    {
        public static IServiceCollection AddSockets(this IServiceCollection services, Action<SocketTransportOptions> configureOptions)
        {
            services.AddLogging();
            services.Configure(configureOptions);
            services.AddSingleton<SocketTransportFactory>();
            return services;
        }
    }
}
