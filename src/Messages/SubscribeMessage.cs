using System.IO;
using System.Collections.Generic;

namespace nMqtt.Messages
{
    /// <summary>
    /// 订阅主题
    /// </summary>
    [MessageType(MessageType.SUBSCRIBE)]
    internal sealed class SubscribeMessage : MqttMessage
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        List<TopicQos> Topics = new List<TopicQos>();
        /// <summary>
        /// 消息ID
        /// </summary>
        public short MessageIdentifier { get; set; }

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

    /// <summary>
    /// 订阅回执
    /// </summary>
    [MessageType(MessageType.SUBACK)]
    internal class SubscribeAckMessage : MqttMessage
    {
        public short MessageIdentifier { get; set; }

        public override void Decode(Stream stream)
        {
            MessageIdentifier = stream.ReadShort();
        }
    }
}