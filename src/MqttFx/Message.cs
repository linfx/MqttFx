using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx
{
    public class Message
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }

        public MqttQos Qos { get; set; }

        public bool Retain { get; set; }
    }

    public static class MessageExtensions
    {
        public static Message ToMessage(this PublishPacket packet)
        {
            return new Message
            {
                Topic = packet.TopicName,
                Payload = packet.Payload,
                Qos = packet.Qos,
                Retain = packet.Retain
            };
        }
    }
}