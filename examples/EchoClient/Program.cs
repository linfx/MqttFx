using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using MqttFx;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EchoClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => true, false));

            var services = new ServiceCollection();
            services.AddMqttFx(options =>
            {
                options.Host = "broker.emqx.io";
                options.Port = 1883;
            });
            var container = services.BuildServiceProvider();

            var client = container.GetService<IMqttClient>();
            //client.Connected += Client_Connected;
            //client.Disconnected += Client_Disconnected;
            //client.MessageReceived += Client_MessageReceived;

            var result = await client.ConnectAsync();
            if (result.Succeeded)
            {
                //var top = "$SYS/brokers/+/clients/#";
                var topic = "testtopic/abc";
                await client.SubscribeAsync(topic);

                //foreach (var rc in rcs)
                //{
                //    Console.WriteLine(rc);
                //}

                //for (int i = 1; i <= 10; i++)
                //{
                //    await client.PublishAsync(topic, Encoding.UTF8.GetBytes($"Hello World!: {i}"));
                //    await Task.Delay(1000);
                //    //Console.ReadKey();
                //}
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
            //var message = e.Message;
            //var payload = Encoding.UTF8.GetString(message.Payload);

            //if (new Regex(@"\$SYS/brokers/.+?/connected").Match(message.Topic).Success)
            //{
            //    //{ "clientid":"mqtt.fx","username":"mqtt.fx","ipaddress":"127.0.0.1","clean_sess":true,"protocol":4,"connack":0,"ts":1540949660}

            //    var obj = JObject.Parse(payload);
            //    Console.WriteLine($"【Client Connected】 client_id:{obj.Value<string>("clientid")}, ipaddress:{obj.Value<string>("ipaddress")}");

            //}
            //else if (new Regex(@"\$SYS/brokers/.+?/disconnected").Match(message.Topic).Success)
            //{
            //    //{"clientid":"mqtt.fx","username":"mqtt.fx","reason":"normal","ts":1540949658}

            //    var obj = JObject.Parse(payload);
            //    Console.WriteLine($"【Client Disconnected】 client_id:{obj.Value<string>("clientid")}");
            //}
            //else
            //{
            //    Console.WriteLine(payload);
            //}
        }
    }
}
