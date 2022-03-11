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

            client.ConnectedAsync += async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                Console.WriteLine("### SUBSCRIBED ###");

                //var subscriptionRequests = new SubscriptionRequestsBuilder()
                //    .WithTopicFilter(f => f.WithTopic("testtopic/a"))
                //    .WithTopicFilter(f => f.WithTopic("testtopic/b").WithAtLeastOnceQoS())
                //    .Build();

                //var subscribeResult = await client.SubscribeAsync(subscriptionRequests);
                var subscribeResult = await client.SubscribeAsync("testopic/bbb", MqttQos.ExactlyOnce);

                foreach (var item in subscribeResult.Items)
                {
                    Console.WriteLine($"+ ResultCode = {item.ResultCode}");
                }
            };

            client.ApplicationMessageReceivedAsync += async message =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {message.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(message.Payload)}");
                Console.WriteLine($"+ QoS = {message.Qos}");
                Console.WriteLine($"+ Retain = {message.Retain}");
                Console.WriteLine();

                await Task.CompletedTask;
            };

            var connectResult = await client.ConnectAsync();
            if (connectResult.Succeeded)
            {
                for (int i = 1; i <= 100; i++)
                {
                    await Task.Delay(500);
                    Console.WriteLine("### Publish Message ###");

                    var mesage = new ApplicationMessageBuilder()
                        .WithTopic("testtopic/ab")
                        .WithPayload($"HelloWorld: {i}")
                        .WithQos(MqttQos.AtLeastOnce)
                        .Build();

                    await client.PublishAsync(mesage);

                    Console.ReadKey();
                }
            }
            else
                Console.WriteLine("Connect Fail!");

            Console.ReadKey();
        }
    }
}
