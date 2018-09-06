using nMqtt.Protocol;
using System.Collections.Generic;
using System.IO;

namespace nMqtt.Packets
{
    /// <summary>
    /// 取消订阅
    /// </summary>
    [PacketType(PacketType.UNSUBSCRIBE)]
    public sealed class UnsubscribePacket : Packet
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
    [PacketType(PacketType.UNSUBACK)]
    public sealed class UnsubscribeAckMessage : Packet
    {
    }
}
