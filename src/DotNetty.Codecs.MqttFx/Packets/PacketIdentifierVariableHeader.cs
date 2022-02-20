using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public class PacketIdentifierVariableHeader : VariableHeader
    {
        /// <summary>
        /// 报文标识符
        /// </summary>
        public ushort PacketIdentifier { get; set; }

        public void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            buffer.WriteUnsignedShort(PacketIdentifier);
        }

        public override void Decode(IByteBuffer buffer, ref int remainingLength)
        {
            PacketIdentifier = buffer.ReadUnsignedShort(ref remainingLength);
            if (PacketIdentifier == 0)
                throw new DecoderException("[MQTT-2.3.1-1]");
        }
    }
}
