using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using nMqtt.Packets;
using nMqtt.Protocol;

namespace nMqtt
{
    public sealed class MqttEncoder : MessageToMessageEncoder<Packet>
    {
        public static readonly MqttEncoder Instance = new MqttEncoder();

        protected override void Encode(IChannelHandlerContext context, Packet message, List<object> output) => DoEncode(context.Allocator, message, output);

        public static void DoEncode(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        {
            IByteBuffer  buffer = bufferAllocator.Buffer();
            try
            {
                //if (packet is PublishPacket publishPacket)
                //{
                //    string topicName = publishPacket.TopicName;
                //    byte[] topicNameBytes = System.Text.Encoding.UTF8.GetBytes(topicName);
                //    int variableHeaderBufferSize = 2 + topicNameBytes.Length + (publishPacket.Qos > MqttQos.AtMostOnce ? 2 : 0);



                //    int payloadBufferSize = publishPacket.Payload.Length;
                //    int variablePartSize = variableHeaderBufferSize + payloadBufferSize;

                //    var buf = Unpooled.Buffer();


                //    buf.WriteShort(topicNameBytes.Length);
                //    buf.WriteBytes(topicNameBytes);
                //    if (publishPacket.Qos > MqttQos.AtMostOnce)
                //    {
                //        buf.WriteUnsignedShort(publishPacket.PacketIdentifier);
                //    }
                //    buf.WriteBytes(publishPacket.Payload, 0, publishPacket.Payload.Length);


                //    buffer.WriteByte(CalculateFirstByteOfFixedHeader(publishPacket));
                //    WriteVariableLengthInt(buffer, variablePartSize);

                //    buffer.WriteBytes(buf);

                //    output.Add(buffer);
                //    buffer = null;
                //}

                packet.Encode(buffer);
                output.Add(buffer);
                buffer = null;

            }
            finally
            {
                buffer?.SafeRelease();
            }
        }


        static int CalculateFirstByteOfFixedHeader(Packet packet)
        {
            int ret = 0;
            ret |= (int)packet.PacketType << 4;
            if (packet.Dup)
            {
                ret |= 0x08;
            }
            ret |= (int)packet.Qos << 1;
            if (packet.Retain)
            {
                ret |= 0x01;
            }
            return ret;
        }

        static void WriteVariableLengthInt(IByteBuffer buffer, int value)
        {
            do
            {
                int digit = value % 128;
                value /= 128;
                if (value > 0)
                {
                    digit |= 0x80;
                }
                buffer.WriteByte(digit);
            }
            while (value > 0);
        }
    }
}