using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 报文抽象类(含报文标识符)
    /// </summary>
    public abstract class PacketWithIdentifier : Packet
    {
        /// <summary>
        /// 可变报头
        /// </summary>
        public PacketIdentifierVariableHeader VariableHeader;

        /// <summary>
        /// 报文抽象类(含报文标识符)
        /// </summary>
        /// <param name="packetType">报文类型</param>
        public PacketWithIdentifier(PacketType packetType)
            : base(packetType) 
        { 
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Encode(IByteBuffer buffer)
        {
            FixedHeader.Encode(buffer, 2);
            VariableHeader.Encode(buffer, FixedHeader);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        public override void Decode(IByteBuffer buffer)
        {
            VariableHeader.Decode(buffer, FixedHeader);
        }
    }
}
