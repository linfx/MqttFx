using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx
{
    /// <summary>
    /// 解码器
    /// </summary>
    public sealed class MqttDecoder : ReplayingDecoder<MqttDecoder.ParseState>
    {
        private readonly bool _isServer;
        private readonly int _maxMessageSize;

        public enum ParseState { Ready, Failed }

        /// <summary>
        /// 解码器
        /// </summary>
        /// <param name="isServer"></param>
        /// <param name="maxMessageSize"></param>
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

        private bool TryDecodePacket(IChannelHandlerContext context, IByteBuffer buffer, out Packet packet)
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

        private Packet DecodePacketInternal(IByteBuffer buffer, byte packetSignature, ref int remainingLength)
        {
            var fixedHeader = new MqttFixedHeader(packetSignature, remainingLength);
            Packet packet = fixedHeader.PacketType switch
            {
                PacketType.CONNECT => new ConnectPacket(),
                PacketType.CONNACK => new ConnAckPacket(),
                PacketType.DISCONNECT => new DisconnectPacket(),
                PacketType.PINGREQ => new PingReqPacket(),
                PacketType.PINGRESP => new PingRespPacket(),
                PacketType.PUBACK => new PubAckPacket(),
                PacketType.PUBCOMP => new PubCompPacket(),
                PacketType.PUBLISH => new PublishPacket(),
                PacketType.PUBREC => new PubRecPacket(),
                PacketType.PUBREL => new PubRelPacket(),
                PacketType.SUBSCRIBE => new SubscribePacket(),
                PacketType.SUBACK => new SubAckPacket(),
                PacketType.UNSUBSCRIBE => new UnsubscribePacket(),
                PacketType.UNSUBACK => new UnsubscribePacket(),
                _ => throw new DecoderException("Unsupported Message Type"),
            };
            packet.FixedHeader = fixedHeader;
            packet.Decode(buffer);
            remainingLength = packet.RemaingLength;
            return packet;
        }

        private bool TryDecodeRemainingLength(IByteBuffer buffer, out int value)
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
    }
}