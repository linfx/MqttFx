using System.IO;

namespace nMqtt.Messages
{
    internal sealed class PublishMessage : MqttMessage
    {
        public string TopicName { get; set; }

        public short MessageIdentifier { get; set; }

        public byte[] Payload { get; set; }

        public PublishMessage()
            : base(MessageType.PUBLISH)
        {
        }

        public override void Encode(Stream stream)
        {
            using (var body = new MemoryStream())
            {
                body.WriteString(TopicName);
                //body.WriteShort(MessageIdentifier);
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

    internal sealed class PublishAckMessage : MqttMessage
    {
        public short MessageIdentifier { get; set; }

        public PublishAckMessage()
            : base(MessageType.PUBACK)
        {
        }
    }

    internal sealed class PublishRecMessage : MqttMessage
    {
        public PublishRecMessage()
            : base(MessageType.PUBREC)
        {
        }
    }

    internal sealed class PublishRelMessage : MqttMessage
    {
        public PublishRelMessage()
            : base(MessageType.PUBREL)
        {
        }
    }

    internal sealed class PublishCompMessage : MqttMessage
    {
        public PublishCompMessage()
            : base(MessageType.PUBCOMP)
        {
        }
    }
}
