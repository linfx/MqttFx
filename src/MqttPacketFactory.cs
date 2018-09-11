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
            var packet = CreatePacket(header);
            packet.Decode(buffer);
            return packet;
        }

        static Packet CreatePacket(FixedHeader header)
        {
            Packet packet;
            switch (header.PacketType)
            {
                case PacketType.CONNECT:
                    packet = new ConnectPacket();
                    break;
                case PacketType.CONNACK:
                    packet = new ConnAckPacket();
                    break;
                case PacketType.DISCONNECT:
                    packet = new DisconnectPacket();
                    break;
                case PacketType.PINGREQ:
                    packet = new PingReqPacket();
                    break;
                case PacketType.PINGRESP:
                    packet = new PingRespPacket();
                    break;
                case PacketType.PUBACK:
                    packet = new PublishAckPacket();
                    break;
                case PacketType.PUBCOMP:
                    packet = new PublishCompPacket();
                    break;
                case PacketType.PUBLISH:
                    packet = new PublishPacket();
                    break;
                case PacketType.PUBREC:
                    packet = new PublishRecPacket();
                    break;
                case PacketType.PUBREL:
                    packet = new PublishRelPacket();
                    break;
                case PacketType.SUBSCRIBE:
                    packet = new SubscribePacket();
                    break;
                case PacketType.SUBACK:
                    packet = new SubAckPacket();
                    break;
                case PacketType.UNSUBSCRIBE:
                    packet = new UnsubscribePacket();
                    break;
                case PacketType.UNSUBACK:
                    packet = new UnsubscribePacket();
                    break;
                default:
                    throw new Exception("Unsupported Message Type");
            }
            packet.FixedHeader = header;
            return packet;
        }
    }
}
