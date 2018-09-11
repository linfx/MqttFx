using nMqtt;
using nMqtt.Messages;
using nMqtt.Protocol;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("118.126.96.166")
                .Build();

            var client = new MqttClient(options);
            client.OnConnected += Connected;
            client.OnMessageReceived += MessageReceived;
            if (await client.ConnectAsync() == ConnectReturnCode.ConnectionAccepted)
            {
                //await client.SubscribeAsync("/World");
                while (true)
                {
                    await client.PublishAsync("/World", Encoding.UTF8.GetBytes("A"));
                    await Task.Delay(1000);
                }
            }
            Console.ReadKey();
        }

        private static void Connected(ConnectReturnCode connectResponse)
        {
            Console.WriteLine("Connected Ssuccessful!, ConnectReturnCode: " + connectResponse);
        }

        private static void MessageReceived(Message message)
        {
            var result = Encoding.UTF8.GetString(message.Payload);
            Console.WriteLine(result);
        }
    }
}
