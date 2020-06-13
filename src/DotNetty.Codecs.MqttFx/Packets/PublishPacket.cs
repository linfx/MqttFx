using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发布消息
    /// </summary>
    public sealed class PublishPacket : Packet
    {
        /// <summary>
        /// 可变报头
        /// </summary>
        public PublishVariableHeader VariableHeader;

        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

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

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                VariableHeader.Encode(buf, FixedHeader);
                buf.WriteBytes(Payload, 0, Payload.Length);
                FixedHeader.Encode(buffer, buf.WriterIndex);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.Release();
            }
        }

        public override void Decode(IByteBuffer buffer)
        {
            VariableHeader.Decode(buffer, FixedHeader);
            if (FixedHeader.RemaingLength > 0)
            {
                Payload = new byte[FixedHeader.RemaingLength];
                buffer.ReadBytes(Payload, 0, FixedHeader.RemaingLength);
                FixedHeader.RemaingLength = 0;
            }
        }
    }
}
