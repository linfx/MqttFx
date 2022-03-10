using DotNetty.Codecs.MqttFx.Packets;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttFx.Client;

public static class MqttClientExtensions
{
    public static Task PublishAsync(this MqttClient mqttClient, string topic, string payload = default, MqttQos qos = MqttQos.AtMostOnce, bool retain = false, CancellationToken cancellationToken = default)
    {
        var payloadBuffer = Encoding.UTF8.GetBytes(payload ?? string.Empty);
        return PublishAsync(mqttClient, topic, payloadBuffer, qos, retain, cancellationToken);
    }

    public static Task PublishAsync(this MqttClient mqttClient, string topic, byte[] payload = default, MqttQos qos = MqttQos.AtMostOnce, bool retain = false, CancellationToken cancellationToken = default)
    {
        if (mqttClient == null)
            throw new ArgumentNullException(nameof(mqttClient));

        if (topic == null)
            throw new ArgumentNullException(nameof(topic));

        var applicationMessage = new ApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQos(qos)
            .WithRetain(retain)
            .Build();

        return mqttClient.PublishAsync(applicationMessage, cancellationToken);
    }

    public static Task<SubscribeResult> SubscribeAsync(this MqttClient mqttClient, TopicFilter topicFilter, CancellationToken cancellationToken = default)
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

    public static Task<SubscribeResult> SubscribeAsync(this MqttClient mqttClient, string topic, MqttQos qos = MqttQos.AtMostOnce, CancellationToken cancellationToken = default)
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
}
