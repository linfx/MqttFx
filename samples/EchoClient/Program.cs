using DotNetty.Codecs.MqttFx.Packets;
using Microsoft.Extensions.DependencyInjection;
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
            var client = container.GetService<IMqttClient>();


            client.UseConnectedHandler(async () =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                var topic = "testtopic/abcd";
                await client.SubscribeAsync(topic);

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
                for (int i = 1; i <= 1; i++)
                {
                    await Task.Delay(500);
                    var topic = "testtopic/abcd";
                    await client.PublishAsync(topic, Encoding.UTF8.GetBytes($"HelloWorld: {i}"), MqttQos.AT_MOST_ONCE);
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
