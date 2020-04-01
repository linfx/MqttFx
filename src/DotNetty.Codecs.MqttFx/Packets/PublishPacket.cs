using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发布消息
    /// </summary>
    public sealed class PublishPacket : PacketWithId
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        public PublishPacket()
            : base(PacketType.PUBLISH)
        { }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="qos">服务质量等级</param>
        /// <param name="dup">重发标志</param>
        /// <param name="retain">保留标志</param>
        public PublishPacket(MqttQos qos, bool dup = false, bool retain = false)
            : this()
        {
            FixedHeader.Qos = qos;
            FixedHeader.Dup = dup;
            FixedHeader.Retain = retain;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

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
            }
            finally
            {
                buf?.Release();
            }
        }

        public override void Decode(IByteBuffer buffer)
        {
            int remainingLength = RemaingLength;

            // variable header
            TopicName = buffer.ReadString(ref remainingLength);
            DecodePacketId(buffer, ref remainingLength);

            // playload
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
    public sealed class PubAckPacket : PacketWithId
    {
        public PubAckPacket(ushort packetId = default)
            : base(PacketType.PUBACK)
        {
            PacketId = packetId;
        }
    }

    /// <summary>
    /// QoS2消息回执
    /// QoS 2 publish received, part 1
    /// </summary>
    public sealed class PubRecPacket : PacketWithId
    {
        public PubRecPacket(ushort packetId = default)
            : base(PacketType.PUBREC)
        {
            PacketId = packetId;
        }
    }

    /// <summary>
    /// QoS2消息释放
    /// QoS 2 publish received, part 2
    /// </summary>
    public sealed class PubRelPacket : PacketWithId
    {
        public PubRelPacket(ushort packetId = default)
            : base(PacketType.PUBREL)
        {
            PacketId = packetId;
        }
    }

    /// <summary>
    /// QoS2消息完成
    /// QoS 2 publish received, part 3
    /// </summary>
    public sealed class PubCompPacket : PacketWithId
    {
        public PubCompPacket(ushort packetId = default)
            : base(PacketType.PUBCOMP)
        {
            PacketId = packetId;
        }
    }
}
