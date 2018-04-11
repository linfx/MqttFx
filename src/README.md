## nMqtt
c# mqtt 3.1.1 client

## QTT协议英文版
http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html

## MQTT协议中文版
https://mcxiaoke.gitbooks.io/mqtt-cn/content/

## Examples
```c#
using System;
using System.Text;
using nMqtt.Messages;


namespace nMqtt.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MqttClient("server", "clientId");
            var state = client.Connect("username");
            if (state == ConnectionState.Connected)
            {
                client.MessageReceived += OnMessageReceived;
                client.Subscribe("a/b", Qos.AtLeastOnce);
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
