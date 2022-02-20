using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public class PacketIdentifierVariableHeader : VariableHeader
    {
        /// <summary>
        /// 报文标识符(Packet Identifier)
        /// </summary>
        public ushort PacketIdentifier { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            buffer.WriteUnsignedShort(PacketIdentifier);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="remainingLength"></param>
        /// <exception cref="DecoderException"></exception>
        public override void Decode(IByteBuffer buffer, ref int remainingLength)
        {
            PacketIdentifier = buffer.ReadUnsignedShort(ref remainingLength);
            if (PacketIdentifier == 0)
                throw new DecoderException("[MQTT-2.3.1-1]");
        }
    }
}
