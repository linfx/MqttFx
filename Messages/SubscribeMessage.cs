using System.IO;
using System.Collections.Generic;

namespace nMqtt.Messages
{
    internal sealed class SubscribeMessage : MqttMessage
    {
        List<TopicQos> Topics = new List<TopicQos>();

        public short MessageIdentifier { get; set; }


        public SubscribeMessage()
            : base(MessageType.SUBSCRIBE)
        {
        }

        public override void Encode(Stream stream)
        {
            using (var body = new MemoryStream())
            {
                body.WriteShort(MessageIdentifier);

                foreach (var item in Topics)
                {
                    body.WriteString(item.Topic);
                    body.WriteByte((byte)item.Qos);
                }

                FixedHeader.RemaingLength = (int)body.Length;
                FixedHeader.WriteTo(stream); 
                body.WriteTo(stream);         
            }
        }

        public void Subscribe(string topic, Qos qos)
        {
            Topics.Add(new TopicQos
            {
                Topic = topic,
                Qos = qos,
            });
        }

        struct TopicQos
        {
            public string Topic { get; set; }
            public Qos Qos { get; set; }
        }
    }

    internal class SubscribeAckMessage : MqttMessage
    {
        public short MessageIdentifier { get; set; }

        public SubscribeAckMessage()
            : base(MessageType.SUBACK)
        {
        }

        public override void Decode(Stream stream)
        {
            MessageIdentifier = stream.ReadShort();
        }
    }
}