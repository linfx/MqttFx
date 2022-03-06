using DotNetty.Codecs.MqttFx.Packets;
using Microsoft.Extensions.DependencyInjection;
using MqttFx;
using MqttFx.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EchoClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddMqttFxClient(options =>
            {
                options.Host = "broker.emqx.io";
                options.Port = 1883;
            });
            var container = services.BuildServiceProvider();

            var client = container.GetService<MqttClient>();

            client.UseConnectedHandler(async () =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                var subscriptionRequests = new SubscriptionRequestsBuilder()
                    .WithTopicFilter( f => f.WithTopic("testtopic/a"))
                    .Build();

                await client.SubscribeAsync(subscriptionRequests);

                Console.WriteLine("### SUBSCRIBED ###");
            });

            client.UseMessageReceivedHandler(message =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {message.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(message.Payload)}");
                Console.WriteLine($"+ QoS = {message.Qos}");
                Console.WriteLine($"+ Retain = {message.Retain}");
                Console.WriteLine();
            });

            var result = await client.ConnectAsync();
            if (result.Succeeded)
            {
                for (int i = 1; i <= 3; i++)
                {
                    await Task.Delay(500);
                    Console.WriteLine("### Publish Message ###");

                    var mesage = new ApplicationMessageBuilder()
                        .WithTopic("testtopic/ab")
                        .WithPayload($"HelloWorld: {i}")
                        .WithQos(MqttQos.AtLeastOnce)
                        .Build();

                    await client.PublishAsync(mesage);
                }
            }
            else
            {
                Console.WriteLine("Connect Fail!");
            }

            Console.ReadKey();
        }
    }
}
