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
                packet = default;
                return false;
            }

            FixedHeader fixedHeader = default;
            fixedHeader.Decode(buffer);
            packet = fixedHeader.PacketType switch
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

            return true;
        }
    }
}