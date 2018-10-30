using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MqttFx;
using DotNetty.Codecs.MqttFx.Packets;

namespace Echo.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddMqttClient(options =>
            {
                options.Server = "118.126.96.166";
            });
            var container = services.BuildServiceProvider();

            var client = container.GetService<MqttClient>();
            client.Connected += Client_Connected;
            client.Disconnected += Client_Disconnected;
            client.MessageReceived += Client_MessageReceived;
            if (await client.ConnectAsync() == ConnectReturnCode.ConnectionAccepted)
            {
                var top = "/World";
                Console.WriteLine("Subscribe:" + top);
                await client.SubscribeAsync(top, MqttQos.AtMostOnce);

                for (int i = 1; i < int.MaxValue; i++)
                {
                    await client.PublishAsync("/World", Encoding.UTF8.GetBytes($"Hello World!: {i}"), MqttQos.AtLeastOnce);
                    await Task.Delay(1000);
                    //Console.ReadKey();
                }
            }
            Console.ReadKey();
        }

        private static void Client_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("Connected Ssuccessful!");
        }

        private static void Client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void Client_MessageReceived(object sender, MqttMessageReceivedEventArgs e)
        {
            var result = Encoding.UTF8.GetString(e.Message.Payload);
            Console.WriteLine(result);
        }
    }
}
