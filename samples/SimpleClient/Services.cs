using DotNetty.Codecs.MqttFx.Packets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MqttFx;
using MqttFx.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleClient
{
    public class Services : IHostedService
    {
        private readonly ILogger logger;
        private readonly MqttClient client;

        public Services(ILogger<Services> logger, MqttClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            client.ConnectedAsync += async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                Console.WriteLine("### SUBSCRIBED ###");

                var subscriptionRequests = new SubscriptionRequestsBuilder()
                    .WithTopicFilter(f => f.WithTopic("testtopic/a"))
                    .WithTopicFilter(f => f.WithTopic("testtopic/b").WithAtLeastOnceQoS())
                    .WithTopicFilter(f => f.WithTopic("testtopic/c").WithExactlyOnceQoS())
                    .Build();

                var subscribeResult = await client.SubscribeAsync(subscriptionRequests);

                foreach (var item in subscribeResult.Items)
                {
                    Console.WriteLine($"+ ResultCode = {item.ResultCode}");
                }

                // online
                var mesage = new ApplicationMessageBuilder()
                    .WithTopic("testtopic/s")
                    .WithPayload($"online")
                    .WithRetain(true)
                    .Build();

                await client.PublishAsync(mesage);
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
                for (int i = 1; i <= 3; i++)
                {
                    await Task.Delay(500);
                    logger.LogInformation("### Publish Message ###");

                    var mesage = new ApplicationMessageBuilder()
                        .WithTopic("testtopic/ab")
                        .WithPayload($"HelloWorld: {i}")
                        .WithQos(MqttQos.AtLeastOnce)
                        .Build();

                    await client.PublishAsync(mesage);
                }
            }
            else
                logger.LogError("Connect Fail!");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
