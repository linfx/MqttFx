using System.Collections.Generic;
using System.IO;

namespace nMqtt.Messages
{
    /// <summary>
    /// 取消订阅
    /// </summary>
    [MessageType(MessageType.UNSUBSCRIBE)]
    public sealed class UnsubscribeMessage : MqttMessage
    {
        List<string> topics = new List<string>();

        public short MessageIdentifier { get; set; }

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

    /// <summary>
    /// 取消订阅回执
    /// </summary>
    [MessageType(MessageType.UNSUBACK)]
    public sealed class UnsubscribeAckMessage : MqttMessage
    {
    }
}
