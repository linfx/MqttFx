using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    public struct PublishVariableHeader
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// 报文标识符
        /// </summary>
        public ushort PacketIdentifier { get; set; }

        public void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            buffer.WriteString(TopicName);
            if (fixedHeader.Qos > MqttQos.AtLeastOnce)
                buffer.WriteUnsignedShort(PacketIdentifier);
        }

        public void Decode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            int remainingLength = fixedHeader.RemaingLength;
            TopicName = buffer.ReadString(ref remainingLength);
            //if (fixedHeader.Qos > MqttQos.AtLeastOnce)
            //{
            //    PacketId = buffer.ReadUnsignedShort(ref remainingLength);
            //    if (PacketId == 0)
            //        throw new DecoderException("[MQTT-2.3.1-1]");
            //}
        }
    }
}
