using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx.Formatter;

public static class PublishPacketFactoryExtensions
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
