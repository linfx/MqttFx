using System;
using MqttFx;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttClient(this IServiceCollection services, Action<MqttClientOptions> optionsAction)
        {
            services.AddLogging();
            services.AddOptions();
            services.Configure(optionsAction);
            services.AddSingleton<MqttClient>();
            return services;
        }
    }
}
