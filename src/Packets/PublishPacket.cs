using System.IO;

namespace nMqtt.Packets
{
    /// <summary>
    /// 发布消息
    /// </summary>
    [PacketType(PacketType.PUBLISH)]
    public sealed class PublishPacket : Packet
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string TopicName { get; set; }
        /// <summary>
        /// 报文标识符
        /// </summary>
        public short MessageIdentifier { get; set; }
        /// <summary>
        /// 有效载荷
        /// </summary>
        public byte[] Payload { get; set; }

        public override void Encode(Stream stream)
        {
            using (var body = new MemoryStream())
            {
                body.WriteString(TopicName);
                body.WriteShort(MessageIdentifier);
                body.Write(Payload, 0, Payload.Length);

                FixedHeader.RemaingLength = (int)body.Length;
                FixedHeader.WriteTo(stream);
                body.WriteTo(stream);
            }
        }

        public override void Decode(Stream stream)
        {
            //variable header
            TopicName = stream.ReadString();
            if (FixedHeader.Qos == MqttQos.AtLeastOnce || FixedHeader.Qos == MqttQos.ExactlyOnce)
                MessageIdentifier = stream.ReadShort();

            //playload
            var len = FixedHeader.RemaingLength - (TopicName.Length + 2);
            Payload = new byte[len];
            stream.Read(Payload, 0, len);
        }
    }

    /// <summary>
    /// 发布回执
    /// QoS level = 1
    /// </summary>
    [PacketType(PacketType.PUBACK)]
    internal sealed class PublishAckPacket : Packet
    {
        public PublishAckPacket(short messageIdentifier = default(short))
        {
            MessageIdentifier = messageIdentifier;
        }

        /// <summary>
        /// 消息ID
        /// </summary>
        public short MessageIdentifier { get; set; }

        public override void Decode(Stream stream)
        {
            MessageIdentifier = stream.ReadShort();
        }
    }

    /// <summary>
    /// QoS2消息回执
    /// QoS 2 publish received, part 1
    /// </summary>
    [PacketType(PacketType.PUBREC)]
    internal sealed class PublishRecPacket : Packet
    {
        public PublishRecPacket(short messageIdentifier = default)
        {
            MessageIdentifier = messageIdentifier;
        }

        /// <summary>
        /// 消息ID
        /// </summary>
        public short MessageIdentifier { get; set; }

        public override void Decode(Stream stream)
        {
            MessageIdentifier = stream.ReadShort();
        }
    }

    /// <summary>
    /// QoS2消息释放
    /// QoS 2 publish received, part 2
    /// </summary>
    [PacketType(PacketType.PUBREL)]
    internal sealed class PublishRelPacket : Packet
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public short MessageIdentifier { get; set; }

        public override void Decode(Stream stream)
        {
            MessageIdentifier = stream.ReadShort();
        }
    }

    /// <summary>
    /// QoS2消息完成
    /// QoS 2 publish received, part 3
    /// </summary>
    [PacketType(PacketType.PUBCOMP)]
    internal sealed class PublishCompPacket : Packet
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public short MessageIdentifier { get; set; }

        public override void Decode(Stream stream)
        {
            MessageIdentifier = stream.ReadShort();
        }
    }
}
