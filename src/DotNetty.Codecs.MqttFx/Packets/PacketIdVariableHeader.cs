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

        public PacketIdVariableHeader() { }

        public PacketIdVariableHeader(ushort packetId)
        {
            PacketId = packetId;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            if (fixedHeader.GetQos() > MqttQos.AT_MOST_ONCE)
                buffer.WriteUnsignedShort(PacketId);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader)
        {
            if (fixedHeader.GetQos() > MqttQos.AT_MOST_ONCE)
            {
                PacketId = buffer.ReadUnsignedShort(ref fixedHeader.RemainingLength);
                if (PacketId == 0)
                    throw new DecoderException("[MQTT-2.3.1-1]");
            }
        }
    }
}
