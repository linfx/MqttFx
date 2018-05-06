using System;
using System.Text;
using Microsoft.Extensions.Logging;
using nMqtt.Messages;

namespace nMqtt.Test
{
    class Program
    {
        static ILogger _logger;
        static MqttClient _client;

        static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole();
            _logger = loggerFactory.CreateLogger<Program>();

            _client = new MqttClient("127.0.0.1", logger: _logger);
            _client.OnMessageReceived += OnMessageReceived;
            _client.ConnectAsync().Wait();

            while (Console.ReadLine() != "c")
            {
                _client.Publish("/World", Encoding.UTF8.GetBytes("测试发送消息"), Qos.AtLeastOnce);
            }

            Console.ReadKey();
        }

        static void OnMessageReceived(MqttMessage message)
        {
            switch (message)
            {
                case ConnAckMessage msg:
                    _logger.LogInformation("---- OnConnAck");
                    _client.Subscribe("/World");
                    break;

                case SubscribeAckMessage msg:
                    _logger.LogInformation("---- OnSubAck");
                    break;

                case PublishMessage msg:
                    _logger.LogInformation("---- OnMessageReceived");
                    _logger.LogInformation(@"topic:{0} data:{1}", msg.TopicName, Encoding.UTF8.GetString(msg.Payload));
                    break;

                default:
                    break;
            }
        }
    }
}