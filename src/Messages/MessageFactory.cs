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

        private static MqttMessage CreateMessage(MessageType msgType)
        {
            switch (msgType)
            {
                case MessageType.CONNECT:
                    return new ConnectMessage();
                case MessageType.CONNACK:
                    return new ConnAckMessage();
                case MessageType.DISCONNECT:
                    return new DisconnectMessage();
                case MessageType.PINGREQ:
                    return new PingReqMessage();
                case MessageType.PINGRESP:
                    return new PingRespMessage();
                case MessageType.PUBACK:
                    return new PublishAckMessage();
                case MessageType.PUBCOMP:
                    return new PublishCompMessage();
                case MessageType.PUBLISH:
                    return new PublishMessage();
                case MessageType.PUBREC:
                    return new PublishRecMessage();
                case MessageType.PUBREL:
                    return new PublishRelMessage();
                case MessageType.SUBSCRIBE:
                    return new SubscribeMessage();
                case MessageType.SUBACK:
                    return new SubscribeAckMessage();
                case MessageType.UNSUBSCRIBE:
                    return new UnsubscribeMessage();
                case MessageType.UNSUBACK:
                    return new UnsubscribeMessage();
                default:
                    throw new Exception("Unsupported Message Type");
            }
        }
    }
}
