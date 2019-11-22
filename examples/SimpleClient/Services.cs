using DotNetty.Codecs.MqttFx.Packets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MqttFx;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleClient
{
    class Services : IHostedService
    {
        private readonly ILogger<Services> _logger;
        private readonly MqttClient _client;

        public Services(ILogger<Services> logger, MqttClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (await _client.ConnectAsync() == ConnectReturnCode.ConnectionAccepted)
            {
                //var top = "/World";
                //_logger.LogInformation("Subscribe:" + top);

                //try
                //{
                //    var subAckPacket = await _client.SubscribeAsync(top, MqttQos.AtMostOnce, CancellationToken.None);

                //    foreach (var rc in subAckPacket.ReturnCodes)
                //    {
                //        Console.WriteLine(rc);
                //    }

                //    for (int i = 1; i < int.MaxValue; i++)
                //    {
                //        await _client.PublishAsync("/World", Encoding.UTF8.GetBytes($"Hello World!: {i}"), MqttQos.AtLeastOnce);
                //        await Task.Delay(1000);
                //    }
                //}
                //catch (MqttTimeoutException ex)
                //{
                //    _logger.LogError(ex, "Subscribe Timeout");
                //}
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
