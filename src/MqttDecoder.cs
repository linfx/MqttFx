using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using nMqtt.Packets;
using System.Collections.Generic;

namespace nMqtt
{
    public sealed class MqttDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                if(!TryDecodePacket(context, input, out Packet packet))
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
            if(!buffer.IsReadable(2))
            {
                packet = null;
                return false;
            }

            packet = MqttPacketFactory.CreatePacket(buffer);

            return true;
        }
    }
}
