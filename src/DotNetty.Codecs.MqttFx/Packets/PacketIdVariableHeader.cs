using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 可变报头(Variable header)
    /// </summary>
    public class PacketIdVariableHeader : VariableHeader
    {
        /// <summary>
        /// 报文标识符(Packet Identifier)
        /// </summary>
        public ushort PacketId { get; set; }

        /// <summary>
        /// 可变报头(Variable header)
        /// </summary>
        public PacketIdVariableHeader() { }

        /// <summary>
        ///  可变报头(Variable header)
        /// </summary>
        /// <param name="packetId">报文标识符(Packet Identifier)</param>
        public PacketIdVariableHeader(ushort packetId)
        {
            PacketId = packetId;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Encode(IByteBuffer buffer)
        {
            buffer.WriteUnsignedShort(PacketId);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader)
        {
            PacketId = buffer.ReadUnsignedShort(ref fixedHeader.RemainingLength);
            if (PacketId == 0)
                throw new DecoderException("SUBSCRIBE, UNSUBSCRIBE, and PUBLISH (in cases where QoS > 0) Control Packets MUST contain a non-zero 16-bit Packet Identifier. [MQTT-2.3.1-1]");
        }
    }
}
