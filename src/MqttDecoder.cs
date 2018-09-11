using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using nMqtt.Packets;
using nMqtt.Protocol;

namespace nMqtt
{
    public sealed class MqttDecoder : ByteToMessageDecoder
    {
        readonly bool _isServer;
        readonly int _maxMessageSize;

        public MqttDecoder(bool isServer, int maxMessageSize)
        {
            _isServer = isServer;
            _maxMessageSize = maxMessageSize;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                if (!TryDecodePacket(context, input, out Packet packet))
                    return;

                output.Add(packet);
            }
            catch (DecoderException)
            {
                input.SkipBytes(input.ReadableBytes);
                throw;
            }
        }

        bool TryDecodePacket(IChannelHandlerContext context, IByteBuffer buffer, out Packet packet)
        {
            if (!buffer.IsReadable(2))
            {
                packet = null;
                return false;
            }

            var fixedHeader = new FixedHeader(buffer);
            switch (fixedHeader.PacketType)
            {
                case PacketType.CONNECT: packet = new ConnectPacket(); break;
                case PacketType.CONNACK: packet = new ConnAckPacket(); break;
                case PacketType.DISCONNECT: packet = new DisconnectPacket(); break;
                case PacketType.PINGREQ: packet = new PingReqPacket(); break;
                case PacketType.PINGRESP: packet = new PingRespPacket(); break;
                case PacketType.PUBACK: packet = new PubAckPacket(); break;
                case PacketType.PUBCOMP: packet = new PubCompPacket(); break;
                case PacketType.PUBLISH: packet = new PublishPacket(); break;
                case PacketType.PUBREC: packet = new PubRecPacket(); break;
                case PacketType.PUBREL: packet = new PubRelPacket(); break;
                case PacketType.SUBSCRIBE: packet = new SubscribePacket(); break;
                case PacketType.SUBACK: packet = new SubAckPacket(); break;
                case PacketType.UNSUBSCRIBE: packet = new UnsubscribePacket(); break;
                case PacketType.UNSUBACK: packet = new UnsubscribePacket(); break;
                default:
                    throw new DecoderException("Unsupported Message Type");
            }
            packet.FixedHeader = fixedHeader;
            packet.Decode(buffer);

            return true;
        }
    }
}