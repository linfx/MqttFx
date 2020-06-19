using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx
{
    /// <summary>
    /// 编码器
    /// </summary>
    public sealed class MqttEncoder : MessageToMessageEncoder<Packet>
    {
        public static readonly MqttEncoder Instance = new MqttEncoder();

        protected override void Encode(IChannelHandlerContext context, Packet message, List<object> output) => DoEncode(context.Allocator, message, output);

        public static void DoEncode(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        {
            IByteBuffer buffer = bufferAllocator.Buffer();
            try
            {
                packet.Encode(buffer);
                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }
    }
}