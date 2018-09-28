using DotNetty.Buffers;
using MqttFx.Protocol;

namespace MqttFx.Packets
{
    /// <summary>
    /// 发布消息
    /// </summary>
    internal sealed class PublishPacket : PacketWithId
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string TopicName { get; set; }
        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

        public PublishPacket() { }

        public PublishPacket(MqttQos qos, bool dup = false, bool retain = false)
        {
            FixedHeader.Qos = qos;
            FixedHeader.Dup = dup;
            FixedHeader.Retain = retain;
        }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteString(TopicName);
                EncodePacketId(buf);
                buf.WriteBytes(Payload, 0, Payload.Length);

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteTo(buffer);
                buffer.WriteBytes(buf);
                buf = null;
            }
            finally
            {
                buf?.Release();
            }
        }

        public override void Decode(IByteBuffer buffer)
        {
            int remainingLength = RemaingLength;

            //variable header
            TopicName = buffer.ReadString(ref remainingLength);
            DecodePacketId(buffer, ref remainingLength);

            //playload
            if (remainingLength > 0)
            {
                Payload = new byte[remainingLength];
                buffer.ReadBytes(Payload, 0, remainingLength);
                remainingLength = 0;
            }

            FixedHeader.RemaingLength = remainingLength;
        }
    }

    /// <summary>
    /// 发布回执
    /// QoS level = 1
    /// </summary>
    internal sealed class PubAckPacket : PacketWithId
    {
        public PubAckPacket(ushort packetId = default)
        {
            PacketId = packetId;
        }
    }

    /// <summary>
    /// QoS2消息回执
    /// QoS 2 publish received, part 1
    /// </summary>
    internal sealed class PubRecPacket : PacketWithId
    {
        public PubRecPacket(ushort packetId = default)
        {
            PacketId = packetId;
        }
    }

    /// <summary>
    /// QoS2消息释放
    /// QoS 2 publish received, part 2
    /// </summary>
    internal sealed class PubRelPacket : PacketWithId
    {
        public PubRelPacket(ushort packetId = default)
        {
            PacketId = packetId;
        }
    }

    /// <summary>
    /// QoS2消息完成
    /// QoS 2 publish received, part 3
    /// </summary>
    internal sealed class PubCompPacket : PacketWithId
    {
        public PubCompPacket(ushort packetId = default)
        {
            PacketId = packetId;
        }
    }
}
