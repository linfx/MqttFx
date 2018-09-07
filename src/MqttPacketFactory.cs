using DotNetty.Buffers;
using nMqtt.Packets;
using nMqtt.Protocol;
using System;

namespace nMqtt
{
    internal static class MqttPacketFactory
    {
        public static Packet CreatePacket(IByteBuffer buffer)
        {
            var header = new FixedHeader(buffer);
            var packet = CreatePacket(header.PacketType);
            packet.FixedHeader = header;
            packet.Decode(buffer);
            return packet;
        }

        private static Packet CreatePacket(PacketType msgType)
        {
            switch (msgType)
            {
                case PacketType.CONNECT:
                    return new ConnectPacket();
                case PacketType.CONNACK:
                    return new ConnAckPacket();
                case PacketType.DISCONNECT:
                    return new DisconnectPacket();
                case PacketType.PINGREQ:
                    return new PingReqPacket();
                case PacketType.PINGRESP:
                    return new PingRespPacket();
                case PacketType.PUBACK:
                    return new PublishAckPacket();
                case PacketType.PUBCOMP:
                    return new PublishCompPacket();
                case PacketType.PUBLISH:
                    return new PublishPacket();
                case PacketType.PUBREC:
                    return new PublishRecPacket();
                case PacketType.PUBREL:
                    return new PublishRelPacket();
                case PacketType.SUBSCRIBE:
                    return new SubscribePacket();
                case PacketType.SUBACK:
                    return new SubscribeAckPacket();
                case PacketType.UNSUBSCRIBE:
                    return new UnsubscribePacket();
                case PacketType.UNSUBACK:
                    return new UnsubscribePacket();
                default:
                    throw new Exception("Unsupported Message Type");
            }
        }
    }
}
