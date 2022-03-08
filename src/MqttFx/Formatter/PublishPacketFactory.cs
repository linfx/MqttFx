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

            // Copy all values to their matching counterparts.
            // The not supported values in MQTT 3.1.1 are not serialized (excluded) later.
            var packet = new PublishPacket
            {
                TopicName = applicationMessage.Topic,
                //Payload = applicationMessage.Payload,
                Qos = applicationMessage.Qos,
                Retain = applicationMessage.Retain,
                Dup = applicationMessage.Dup,
                //ContentType = applicationMessage.ContentType,
                //CorrelationData = applicationMessage.CorrelationData,
                //MessageExpiryInterval = applicationMessage.MessageExpiryInterval,
                //PayloadFormatIndicator = applicationMessage.PayloadFormatIndicator,
                //ResponseTopic = applicationMessage.ResponseTopic,
                //TopicAlias = applicationMessage.TopicAlias,
                //SubscriptionIdentifiers = applicationMessage.SubscriptionIdentifiers,
                //UserProperties = applicationMessage.UserProperties
            };
            ((PublishPayload)packet.Payload).Data = applicationMessage.Payload;

            return packet;
        }
    }
}
