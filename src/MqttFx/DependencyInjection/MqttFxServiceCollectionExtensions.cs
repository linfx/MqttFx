using System;
using MqttFx;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MqttFxServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttClient(this IServiceCollection services, Action<MqttClientOptions> optionsAction)
        {
            if(optionsAction == null)
            {
                throw new ArgumentNullException("optionsAction");
            }

            services.AddLogging();
            services.AddOptions();
            services.Configure(optionsAction);
            services.AddSingleton<IMqttClient, MqttClient>();
            return services;
        }
    }
}
