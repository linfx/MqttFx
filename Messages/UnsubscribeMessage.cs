using System.Collections.Generic;
using System.IO;

namespace nMqtt.Messages
{
    internal sealed class UnsubscribeMessage : MqttMessage
    {
        List<string> topics = new List<string>();

        public short MessageIdentifier { get; set; }

        public UnsubscribeMessage()
            : base(MessageType.UNSUBSCRIBE)
        {
        }

        public override void Encode(Stream stream)
        {
            using (var body = new MemoryStream())
            {
                body.WriteShort(MessageIdentifier);

                foreach (var item in topics)
                {
                    body.WriteString(item);
                }

                FixedHeader.RemaingLength = (int)body.Length;
                FixedHeader.WriteTo(stream);
                body.WriteTo(stream);
            }
        }

        public void Unsubscribe(string topic)
        {
            topics.Add(topic);
        }
    }

    internal sealed class UnsubscribeAckMessage : MqttMessage
    {
        public UnsubscribeAckMessage()
            : base(MessageType.UNSUBACK)
        {
        }
    }
}
