using DotNetty.Codecs.MqttFx.Packets;
using MqttFx.Client.Handlers;
using System;
using System.Text;
using System.Threading;

namespace MqttFx.Client
{
    public static class MqttClientExtensions
    {
        public static MqttClient UseConnectedHandler(this MqttClient client, Action handler)
        {
            return client.UseConnectedHandler(new MqttClientConnectedHandler(handler));
        }

        public static MqttClient UseConnectedHandler(this MqttClient client, IMqttClientConnectedHandler handler)
        {
            client.ConnectedHandler = handler;
            return client;
        }

        public static MqttClient UseDisconnectedHandler(this MqttClient client, Action handler)
        {
            return client.UseDisconnectedHandler(new MqttClientDisconnectedHandler(handler));
        }

        public static MqttClient UseDisconnectedHandler(this MqttClient client, IMqttClientDisconnectedHandler handler)
        {
            client.DisconnectedHandler = handler;
            return client;
        }

        public static MqttClient UseMessageReceivedHandler(this MqttClient client, Action<ApplicationMessage> handler)
        {
            return client.UseApplicationMessageReceivedHandler(new MessageReceivedHandler(handler));
        }

        public static MqttClient UseApplicationMessageReceivedHandler(this MqttClient client, IMessageReceivedHandler handler)
        {
            client.MessageReceivedHandler = handler;
            return client;
        }

        public static MqttClient PublishAsync(this MqttClient client, string topic, string payload = default, MqttQos qos = MqttQos.AT_MOST_ONCE, bool retain = false, CancellationToken cancellationToken = default)
        {
            var payloadBuffer = Encoding.UTF8.GetBytes(payload ?? string.Empty);
            return PublishAsync(client, topic, payloadBuffer, qos, retain, cancellationToken);
        }

        public static MqttClient PublishAsync(this MqttClient client, string topic, byte[] payload = default, MqttQos qos = MqttQos.AT_MOST_ONCE, bool retain = false, CancellationToken cancellationToken = default)
        {
            var applicationMessage = new ApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                //.WithRetainFlag(retain)
                //.WithPayload(qualityOfServiceLevel)
                .Build();

            client.PublishAsync(applicationMessage, cancellationToken);
            return client;
        }
    }
}