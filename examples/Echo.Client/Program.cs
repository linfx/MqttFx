using nMqtt;
using nMqtt.Messages;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MqttClient client = new MqttClient();
            client.OnMessageReceived += MessageReceived;
            await client.ConnectAsync();

            client.

            Console.WriteLine("连接成功");
        }

        private static void MessageReceived(Message message)
        {
            var result = Encoding.UTF8.GetString(message.Payload);
            Console.WriteLine(result);
        }
    }
}
