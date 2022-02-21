using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 可变报头
    /// </summary>
    public class PublishVariableHeader : VariableHeader
    {
        /// <summary>
        /// 主题名(UTF-8编码的字符串)(Topic Name)
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// 报文标识符(Packet Identifier)
        /// 只有当QoS等级是1或2时，报文标识符（Packet Identifier）字段才能出现在PUBLISH报文中。
        /// </summary>
        public ushort PacketId { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
        {
            buffer.WriteString(TopicName);
            if (fixedHeader.GetQos() > MqttQos.AT_LEAST_ONCE)
                buffer.WriteUnsignedShort(PacketId);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fixedHeader"></param>
        public override void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader)
        {
            TopicName = buffer.ReadString(ref fixedHeader.RemainingLength);
            if (fixedHeader.GetQos() > MqttQos.AT_LEAST_ONCE)
            {
                PacketId = buffer.ReadUnsignedShort(ref fixedHeader.RemainingLength);
                if (PacketId == 0)
                    throw new DecoderException("[MQTT-2.3.1-1]");
            }
        }
    }
}
