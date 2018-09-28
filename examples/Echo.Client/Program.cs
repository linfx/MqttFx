using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MqttFx;
using MqttFx.Packets;

namespace Echo.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var options = new MqttClientOptionsBuilder()
            //    .WithTcpServer("118.126.96.166")
            //    .WithClientId("nmqtt_client")
            //    .WithCredentials("linfx", "123456")
            //    .Build();

            var services = new ServiceCollection();
            services.AddMqttClient(options =>
            {
                options.Server = "118.126.96.166";
            });
            var container = services.BuildServiceProvider();

            var client = container.GetService<MqttClient>();
            client.OnConnected += Connected;
            client.OnDisconnected += Disconnected;
            client.OnMessageReceived += MessageReceived;
            if (await client.ConnectAsync() == ConnectReturnCode.ConnectionAccepted)
            {
                var top = "/World";
                Console.WriteLine("Subscribe:" + top);
                Console.Write("SubscribeReturnCode: ");
                var r = await client.SubscribeAsync(top);
                Console.WriteLine(r.ReturnCodes);
                //while (true)
                //{
                //    await client.PublishAsync("/World", Encoding.UTF8.GetBytes("Hello World!"), MqttQos.AtMostOnce);
                //    await Task.Delay(2000);
                //}
            }
            Console.ReadKey();
        }

        private static void Connected(ConnectReturnCode connectResponse)
        {
            Console.WriteLine("Connected Ssuccessful!, ConnectReturnCode: " + connectResponse);
        }

        private static void Disconnected()
        {
            Console.WriteLine("Disconnected");
        }

        private static void MessageReceived(Message message)
        {
            var result = Encoding.UTF8.GetString(message.Payload);
            Console.WriteLine(result);
        }
    }
}
