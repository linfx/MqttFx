//using Microsoft.Extensions.Logging;
//using MQTTnet;
//using MQTTnet.Client;
//using System;
//using System.Text;
//using System.Threading.Tasks;

//namespace ConsoleApp1
//{
//    class Program
//    {
//        //static ILogger _logger;
//        //static MqttClient _client;

//        static async Task Main(string[] args)
//        {

//            //var services = new ServiceCollection();
//            //services.AddSockets(options =>
//            //{
//            //    options.IOQueueCount = 1;
//            //});
//            //var container = services.BuildServiceProvider();

//            //var f = container.GetService<SocketTransportFactory>();
//            //var transport = f.Create();
//            //await transport.BindAsync();


//            //var loggerFactory = new LoggerFactory()
//            //    .AddConsole();
//            //_logger = loggerFactory.CreateLogger<Program>();

//            //_client = new MqttClient("118.126.96.166", logger: _logger);
//            //_client.OnMessageReceived += OnMessageReceived;
//            //await _client.ConnectAsync();



//            //while (Console.ReadLine() != "c")
//            //{
//            //    _logger.LogInformation("测试发送消息");
//            //    _client.Publish("/World", Encoding.UTF8.GetBytes("测试发送消息"), MqttQos.AtLeastOnce);
//            //}

//            //IMqttClient client = new MqttClient("118.126.96.166", "123456");


//            var opt = new MqttClientOptionsBuilder().WithTcpServer("118.126.96.166").Build();

//            var factory = new MqttFactory();
//            var client = factory.CreateMqttClient();
//            client.ApplicationMessageReceived += Client_ApplicationMessageReceived;
//            await client.ConnectAsync(opt);


//            await client.SubscribeAsync("MQTTnet.RPC/Add");




//            while (true)
//            {
//                var rpc = new MqttRpcClient(client);

//                Console.WriteLine("调用远程:");
//                var result = await rpc.ExecuteAsync(TimeSpan.FromMinutes(10), "Add", Encoding.UTF8.GetBytes("1"), MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
//                var r = Encoding.UTF8.GetString(result);
//                Console.WriteLine("远程结果:" + r);

//                Console.ReadKey();
//            }
            
//        }

//        private static void Client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
//        {
//            var result = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
//            Console.WriteLine(result);
//        }

//        //static void OnMessageReceived(MqttMessage message)
//        //{
//        //    switch (message)
//        //    {
//        //        case ConnAckMessage msg:
//        //            _logger.LogInformation("---- OnConnAck");
//        //            _client.Subscribe("/World");
//        //            break;

//        //        case SubscribeAckMessage msg:
//        //            _logger.LogInformation("---- OnSubAck");
//        //            break;

//        //        case PublishMessage msg:
//        //            _logger.LogInformation("---- 收到消息");
//        //            _logger.LogInformation(@"topic:{0} data:{1}", msg.TopicName, Encoding.UTF8.GetString(msg.Payload));
//        //            break;

//        //        default:
//        //            break;
//        //    }
//        //}



//    }

//    //public static class Ext
//    //{
//    //    public static IServiceCollection AddSockets(this IServiceCollection services, Action<SocketTransportOptions> configureOptions)
//    //    {
//    //        services.AddLogging();
//    //        services.Configure(configureOptions);
//    //        services.AddSingleton<SocketTransportFactory>();
//    //        return services;
//    //    }
//    //}
//}
