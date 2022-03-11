using DotNetty.Codecs.MqttFx.Packets;
using System.Text;

namespace MqttFx;

public sealed class ApplicationMessageBuilder
{
    string _topic;
    byte[] _payload;
    MqttQos _qos = MqttQos.AtMostOnce;
    bool _retain;

    public ApplicationMessage Build()
    {
        var message = new ApplicationMessage
        {
            Topic = _topic,
            Payload = _payload,
            Qos = _qos,
            Retain = _retain,
        };
        return message;
    }

    public ApplicationMessageBuilder WithPayload(byte[] payload)
    {
        _payload = payload;
        return this;
    }

    public ApplicationMessageBuilder WithPayload(string payload)
    {
        if (payload == null)
        {
            _payload = null;
            return this;
        }

        _payload = string.IsNullOrEmpty(payload) ? null : Encoding.UTF8.GetBytes(payload);
        return this;
    }

    /// <summary>
    ///     The MQTT topic.
    ///     In MQTT, the word topic refers to an UTF-8 string that the broker uses to filter messages for each connected
    ///     client.
    ///     The topic consists of one or more topic levels. Each topic level is separated by a forward slash (topic level
    ///     separator).
    /// </summary>
    public ApplicationMessageBuilder WithTopic(string topic)
    {
        _topic = topic;
        return this;
    }

    /// <summary>
    ///     The quality of service level.
    ///     The Quality of Service (QoS) level is an agreement between the sender of a message and the receiver of a message
    ///     that defines the guarantee of delivery for a specific message.
    ///     There are 3 QoS levels in MQTT:
    ///     - At most once  (0): Message gets delivered no time, once or multiple times.
    ///     - At least once (1): Message gets delivered at least once (one time or more often).
    ///     - Exactly once  (2): Message gets delivered exactly once (It's ensured that the message only comes once).
    /// </summary>
    public ApplicationMessageBuilder WithQos(MqttQos qos)
    {
        _qos = qos;
        return this;
    }

    /// <summary>
    ///     A value indicating whether the message should be retained or not.
    ///     A retained message is a normal MQTT message with the retained flag set to true.
    ///     The broker stores the last retained message and the corresponding QoS for that topic.
    /// </summary>
    public ApplicationMessageBuilder WithRetain(bool value = true)
    {
        _retain = value;
        return this;
    }
}

    public static class ApplicationMessageExtensions
    {
        public static ApplicationMessage ToApplicationMessage(this PublishPacket packet)
        {
            return new ApplicationMessage
            {
                Qos = packet.Qos,
                Retain = packet.Retain,
                Topic = packet.TopicName,
                Payload = (PublishPayload)packet.Payload
            };
        }
    }
}
