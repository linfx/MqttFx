using DotNetty.Codecs.MqttFx.Packets;
using System;

namespace MqttFx.Formatter
{
    class PublishPacketFactory
    {
        public static PublishPacket Create(ApplicationMessage applicationMessage)
        {
            if (applicationMessage == null)
                throw new ArgumentNullException(nameof(applicationMessage));

            var packet = new PublishPacket
            {
                TopicName = applicationMessage.Topic,
                Qos = applicationMessage.Qos,
                Dup = applicationMessage.Dup,
                Retain = applicationMessage.Retain
            };
            packet.SetPayload(applicationMessage.Payload);

            return packet;
        }
    }
}
