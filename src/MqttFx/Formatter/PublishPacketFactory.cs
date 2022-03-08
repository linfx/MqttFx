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

            var packet = new PublishPacket(applicationMessage.Qos, applicationMessage.Dup, applicationMessage.Retain)
            {
                TopicName = applicationMessage.Topic,
            };
            packet.SetPayload(applicationMessage.Payload);

            return packet;
        }
    }
}
