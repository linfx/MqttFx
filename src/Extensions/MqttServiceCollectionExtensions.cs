using System;
using MqttFx;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MqttServiceCollectionExtensions
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
