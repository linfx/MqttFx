using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx;

/// <summary>
/// 编码器
/// </summary>
public sealed class MqttEncoder : MessageToMessageEncoder<Packet>
{
    public static readonly MqttEncoder Instance = new MqttEncoder();

    protected override void Encode(IChannelHandlerContext context, Packet packet, List<object> output)
    {
        DoEncode(context.Allocator, packet, output);
    }

    public static void DoEncode(IByteBufferAllocator allocator, Packet packet, List<object> output)
    {
        var buffer = allocator.Buffer();
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
