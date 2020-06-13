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
            : base(PacketType.PUBLISH) { }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="qos">服务质量等级</param>
        /// <param name="dup">重发标志</param>
        /// <param name="retain">保留标志</param>
        public PublishPacket(MqttQos qos, bool dup = false, bool retain = false) : this()
        {
            FixedHeader.Qos = qos;
            FixedHeader.Dup = dup;
            FixedHeader.Retain = retain;
        }

        /// <summary>
        /// 可变报头
        /// </summary>
        public PublishVariableHeader VariableHeader;

        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteString(VariableHeader.TopicName);
                WritePacketId(buf);
                buf.WriteBytes(Payload, 0, Payload.Length);

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.WriteFixedHeader(buffer);
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
            VariableHeader.TopicName = buffer.ReadString(ref remainingLength);
            ReadPacketId(buffer, ref remainingLength);

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
}
