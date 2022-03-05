using DotNetty.Codecs.MqttFx.Packets;
using System.Text;

namespace MqttFx
{
    public sealed class ApplicationMessageBuilder
    {
        string _topic;
        byte[] _payload;
        MqttQos _qos = MqttQos.AT_MOST_ONCE;
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

        public ApplicationMessageBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }
    }

    public static class MessageExtensions
    {
        public static ApplicationMessage ToMessage(this PublishPacket packet)
        {
            return new ApplicationMessage
            {
                Qos = packet.Qos,
                Retain = packet.Retain,
                Topic = packet.TopicName,
                Payload = ((PublishPayload)packet.Payload).Data
            };
        }
    }
}
