# MqttFx

c# mqtt 3.1.1 client

官方交流群: 241445317

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
            services.AddMqttClient(options =>
            {
                options.Host = "118.126.96.166";
            });
            var container = services.BuildServiceProvider();

            var client = container.GetService<MqttClient>();
            client.Connected += Client_Connected;
            client.Disconnected += Client_Disconnected;
            client.MessageReceived += Client_MessageReceived;
            if (await client.ConnectAsync() == ConnectReturnCode.ConnectionAccepted)
            {
                var top = "$SYS/brokers/+/clients/#";
                Console.WriteLine("Subscribe:" + top);

                var rcs = (await client.SubscribeAsync(top, MqttQos.AtMostOnce)).ReturnCodes;

                foreach (var rc in rcs)
                {
                    Console.WriteLine(rc);
                }

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
            //$SYS/brokers/+/clients/+/connected
            //$SYS/brokers/+/clients/+/disconnected
            //$SYS/brokers/+/clients/#
            var message = e.Message;
            var payload = Encoding.UTF8.GetString(message.Payload);

            if (new Regex(@"\$SYS/brokers/.+?/connected").Match(message.Topic).Success)
            {
                //{ "clientid":"mqtt.fx","username":"mqtt.fx","ipaddress":"127.0.0.1","clean_sess":true,"protocol":4,"connack":0,"ts":1540949660}

                var obj = JObject.Parse(payload);
                Console.WriteLine($"【Client Connected】 client_id:{obj.Value<string>("clientid")}, ipaddress:{obj.Value<string>("ipaddress")}");

            }
            else if (new Regex(@"\$SYS/brokers/.+?/disconnected").Match(message.Topic).Success)
            {
                //{"clientid":"mqtt.fx","username":"mqtt.fx","reason":"normal","ts":1540949658}

                var obj = JObject.Parse(payload);
                Console.WriteLine($"【Client Disconnected】 client_id:{obj.Value<string>("clientid")}");
            }
            else
            {
                Console.WriteLine(payload);
            }
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