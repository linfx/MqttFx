using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct PacketIdentifierVariableHeader
    {
        /// <summary>
        /// 报文标识符
        /// </summary>
        public ushort PacketIdentifier { get; set; }

        public void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            buffer.WriteUnsignedShort(PacketIdentifier);
        }

        public void Decode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            int remainingLength = fixedHeader.RemaingLength;
            PacketIdentifier = buffer.ReadUnsignedShort(ref remainingLength);
            if (PacketIdentifier == 0)
                throw new DecoderException("[MQTT-2.3.1-1]");
        }
    }
}
