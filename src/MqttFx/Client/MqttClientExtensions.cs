using DotNetty.Codecs.MqttFx.Packets;
using MqttFx.Client.Handlers;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx.Client
{
    public static class MqttClientExtensions
    {
        public static Task PublishAsync(this MqttClient mqttClient, string topic, string payload = default, MqttQos qos = MqttQos.AT_MOST_ONCE, bool retain = false, CancellationToken cancellationToken = default)
        {
            var payloadBuffer = Encoding.UTF8.GetBytes(payload ?? string.Empty);
            return PublishAsync(mqttClient, topic, payloadBuffer, qos, retain, cancellationToken);
        }

        public static Task PublishAsync(this MqttClient mqttClient, string topic, byte[] payload = default, MqttQos qos = MqttQos.AT_MOST_ONCE, bool retain = false, CancellationToken cancellationToken = default)
        {
            if (mqttClient == null)
                throw new ArgumentNullException(nameof(mqttClient));

            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var applicationMessage = new ApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQos(qos)
            .WithRetainFlag(retain)
            .Build();

            return mqttClient.PublishAsync(applicationMessage, cancellationToken);
        }

        public static Task SubscribeAsync(this MqttClient mqttClient, TopicFilter topicFilter, CancellationToken cancellationToken = default)
        {
            if (mqttClient == null)
                throw new ArgumentNullException(nameof(mqttClient));

            if (topicFilter == null)
                throw new ArgumentNullException(nameof(topicFilter));

            var subscribeOptions = new SubscriptionRequestsBuilder()
                .WithTopicFilter(topicFilter)
                .Build();

            return mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
        }

        public static Task SubscribeAsync(this MqttClient mqttClient, string topic, MqttQos qos = MqttQos.AT_MOST_ONCE, CancellationToken cancellationToken = default)
        {
            if (mqttClient == null)
                throw new ArgumentNullException(nameof(mqttClient));

            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var subscriptionRequests = new SubscriptionRequestsBuilder()
                .WithTopicFilter(topic, qos)
                .Build();

            return mqttClient.SubscribeAsync(subscriptionRequests, cancellationToken);
        }

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
    }
}