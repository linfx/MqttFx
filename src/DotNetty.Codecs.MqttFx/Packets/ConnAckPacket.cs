using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接报文回执
    /// </summary>
    public sealed class ConnAckPacket : Packet
    {
        /// <summary>
        /// 可变报头
        /// </summary>
        public ConnAckVariableHeader VariableHeader;

        /// <summary>
        /// 连接报文回执
        /// </summary>
        public ConnAckPacket()
            : base(PacketType.CONNACK) { }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            FixedHeader.Encode(buffer, 2);
            VariableHeader.Encode(buffer);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Decode(IByteBuffer buffer)
        {
            VariableHeader.Decode(buffer);
        }
    }
}