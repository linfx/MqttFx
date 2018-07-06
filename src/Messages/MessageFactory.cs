using System;
using System.IO;

namespace nMqtt.Messages
{
    internal static class MessageFactory
    {
        public static MqttMessage CreateMessage(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var header = new FixedHeader(stream);
                var msg = CreateMessage(header.MessageType);
                msg.FixedHeader = header;
                msg.Decode(stream);
                return msg;
            }
        }

        private static MqttMessage CreateMessage(MqttMessageType msgType)
        {
            switch (msgType)
            {
                case MqttMessageType.CONNECT:
                    return new ConnectMessage();
                case MqttMessageType.CONNACK:
                    return new ConnAckMessage();
                case MqttMessageType.DISCONNECT:
                    return new DisconnectMessage();
                case MqttMessageType.PINGREQ:
                    return new PingReqMessage();
                case MqttMessageType.PINGRESP:
                    return new PingRespMessage();
                case MqttMessageType.PUBACK:
                    return new PublishAckMessage();
                case MqttMessageType.PUBCOMP:
                    return new PublishCompMessage();
                case MqttMessageType.PUBLISH:
                    return new PublishMessage();
                case MqttMessageType.PUBREC:
                    return new PublishRecMessage();
                case MqttMessageType.PUBREL:
                    return new PublishRelMessage();
                case MqttMessageType.SUBSCRIBE:
                    return new SubscribeMessage();
                case MqttMessageType.SUBACK:
                    return new SubscribeAckMessage();
                case MqttMessageType.UNSUBSCRIBE:
                    return new UnsubscribeMessage();
                case MqttMessageType.UNSUBACK:
                    return new UnsubscribeMessage();
                default:
                    throw new Exception("Unsupported Message Type");
            }
        }
    }
}
