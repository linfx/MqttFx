using System;
using System.Text;
using System.Threading;
using nMqtt.Messages;

namespace nMqtt.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MqttClient("127.0.0.1");
            var state = client.ConnectAsync().Result;
            if (state == ConnectionState.Connected)
            {
                client.MessageReceived += OnMessageReceived;
                client.Subscribe("/World", Qos.ExactlyOnce);

                for (int i = 0; i < 10; i++)
                {
                    client.Publish("/World", Encoding.UTF8.GetBytes("测试发送消息_" + i.ToString()), Qos.ExactlyOnce);
                    Thread.Sleep(100);
                }
            }
            Console.ReadKey();
        }

        static void OnMessageReceived(string topic, byte[] data)
        {
            Console.WriteLine("-------------------");
            Console.WriteLine("topic:{0}", topic);
            Console.WriteLine("data:{0}", Encoding.UTF8.GetString(data));
        }
    }
}