using System.IO;

namespace nMqtt.Messages
{
    /// <summary>
    /// 发布消息
    /// </summary>
    [MessageType(MessageType.PUBLISH)]
    public sealed class PublishMessage : MqttMessage
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
            if (FixedHeader.Qos == Qos.AtLeastOnce || FixedHeader.Qos == Qos.ExactlyOnce)
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
    [MessageType(MessageType.PUBACK)]
    internal sealed class PublishAckMessage : MqttMessage
    {
        public PublishAckMessage(short messageIdentifier = default(short))
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
    [MessageType(MessageType.PUBREC)]
    internal sealed class PublishRecMessage : MqttMessage
    {
        public PublishRecMessage(short messageIdentifier = default(short))
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
    [MessageType(MessageType.PUBREL)]
    internal sealed class PublishRelMessage : MqttMessage
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
    [MessageType(MessageType.PUBCOMP)]
    internal sealed class PublishCompMessage : MqttMessage
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
