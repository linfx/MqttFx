using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Codecs.MqttFx.Packets;
using System;

namespace DotNetty.Codecs.MqttFx
{
    /// <summary>
    /// 解码器
    /// </summary>
    public sealed class MqttDecoder : ReplayingDecoder<MqttDecoder.ParseState>
    {
        public enum ParseState
        {
            Ready,
            Failed
        }

        readonly bool _isServer;
        readonly int _maxMessageSize;

        public MqttDecoder(bool isServer, int maxMessageSize)
            : base(ParseState.Ready)
        {
            _isServer = isServer;
            _maxMessageSize = maxMessageSize;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                switch (State)
                {
                    case ParseState.Ready:
                        if (!TryDecodePacket(context, input, out Packet packet))
                        {
                            RequestReplay();
                            return;
                        }
                        output.Add(packet);
                        Checkpoint();
                        break;
                    case ParseState.Failed:
                        // read out data until connection is closed
                        input.SkipBytes(input.ReadableBytes);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (DecoderException)
            {
                input.SkipBytes(input.ReadableBytes);
                Checkpoint(ParseState.Failed);
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

            byte signature = buffer.ReadByte();

            if (!TryDecodeRemainingLength(buffer, out int remainingLength) || !buffer.IsReadable(remainingLength))
            {
                packet = null;
                return false;
            }

            packet = DecodePacketInternal(buffer, signature, ref remainingLength);

            if (remainingLength > 0)
                throw new DecoderException($"Declared remaining length is bigger than packet data size by {remainingLength}.");

            return true;
        }

        Packet DecodePacketInternal(IByteBuffer buffer, byte packetSignature, ref int remainingLength)
        {
            Packet packet;
            var fixedHeader = new FixedHeader(packetSignature, remainingLength);
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
            remainingLength = packet.RemaingLength;
            return packet;
        }

        bool TryDecodeRemainingLength(IByteBuffer buffer, out int value)
        {
            int readable = buffer.ReadableBytes;

            int result = 0;
            int multiplier = 1;
            byte digit;
            int read = 0;
            do
            {
                if (readable < read + 1)
                {
                    value = default;
                    return false;
                }
                digit = buffer.ReadByte();
                result += (digit & 0x7f) * multiplier;
                multiplier <<= 7;
                read++;
            }
            while ((digit & 0x80) != 0 && read < 4);

            if (read == 4 && (digit & 0x80) != 0)
                throw new DecoderException("Remaining length exceeds 4 bytes in length");

            int completeMessageSize = result + 1 + read;
            if (completeMessageSize > _maxMessageSize)
                throw new DecoderException("Message is too big: " + completeMessageSize);

            value = result;
            return true;
        }

        //static int DecodeRemainingLength(IByteBuffer buffer)
        //{
        //    byte encodedByte;
        //    var multiplier = 1;
        //    var remainingLength = 0;
        //    do
        //    {
        //        encodedByte = buffer.ReadByte();
        //        remainingLength += (encodedByte & 0x7f) * multiplier;
        //        multiplier *= 0x80;
        //    } while ((encodedByte & 0x80) != 0);

        //    return remainingLength;
        //}
    }
}