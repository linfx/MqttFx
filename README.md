# MqttFx

c# mqtt 3.1.1 client

***

## Install

`PM> Install-Package MqttFx`


## Samples
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
                options.WillTopic = "testtopic/c";
                options.WillPayload = Encoding.UTF8.GetBytes("offline");
                options.WillRetain = true;
            });
            var container = services.BuildServiceProvider();

            var client = container.GetService<MqttClient>();

            client.ConnectedAsync += async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                Console.WriteLine("### SUBSCRIBED ###");

                var subscriptionRequests = new SubscriptionRequestsBuilder()
                    .WithTopicFilter(f => f.WithTopic("testtopic/a"))
                    .WithTopicFilter(f => f.WithTopic("testtopic/b").WithAtLeastOnceQoS())
                    .Build();

                var subscribeResult = await client.SubscribeAsync(subscriptionRequests);

                foreach (var item in subscribeResult.Items)
                {
                    Console.WriteLine($"+ ResultCode = {item.ResultCode}");
                }

                // online
                var mesage = new ApplicationMessageBuilder()
                    .WithTopic("testtopic/c")
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
                    Console.WriteLine("### Publish Message ###");

                    var mesage = new ApplicationMessageBuilder()
                        .WithTopic("testtopic/ab")
                        .WithPayload($"HelloWorld: {i}")
                        .WithQos(MqttQos.AtLeastOnce)
                        .Build();

                    await client.PublishAsync(mesage);
                }
            }
            else
                Console.WriteLine("Connect Fail!");

            Console.ReadKey();
        }
    }

```

## MQTT 规范

你可以通过以下链接了解与查阅 MQTT 协议:

[MQTT 协议中文版](https://mcxiaoke.gitbooks.io/mqtt-cn/content/)

[MQTT Version 3.1.1](https://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html)

[MQTT Version 5.0](https://docs.oasis-open.org/mqtt/mqtt/v5.0/cs02/mqtt-v5.0-cs02.html)

[MQTT SN](https://www.oasis-open.org/committees/download.php/66091/MQTT-SN_spec_v1.2.pdf)