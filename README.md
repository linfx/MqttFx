# MqttFx

c# mqtt 3.1.1 client

***

## Install

`PM> Install-Package MqttFx`


## Examples
```c#

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

                var topic = "testtopic/a";
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
                for (int i = 1; i <= 3; i++)
                {
                    await Task.Delay(500);
                    Console.WriteLine("### Publish Message ###");
                    var topic = "testtopic/ab";
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

```


## EMQ  百万级分布式开源物联网MQTT消息服务器
http://www.emqtt.com/

***************************

## 概览
#### MQTT是一个轻量的发布订阅模式消息传输协议，专门针对低带宽和不稳定网络环境的物联网应用设计
```
MQTT 官网:            http://mqtt.org
MQTT V3.1.1协议规范:  http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
MQTT 协议英文版:      http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html
MQTT 协议中文版:      https://mcxiaoke.gitbooks.io/mqtt-cn/content/
MQTT 协议中文版:      https://legacy.gitbook.com/book/mcxiaoke/mqtt-cn

```
