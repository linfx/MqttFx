using System.IO;
using System.Collections.Generic;
using DotNetty.Buffers;
using nMqtt.Protocol;

namespace nMqtt.Packets
{
    /// <summary>
    /// 订阅主题
    /// </summary>
    [PacketType(PacketType.SUBSCRIBE)]
    public sealed class SubscribePacket : Packet
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        List<TopicQos> Topics = new List<TopicQos>();

        /// <summary>
        /// 消息ID
        /// </summary>
        public short MessageIdentifier { get; set; }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteShort(MessageIdentifier);

                foreach (var item in Topics)
                {
                    buf.WriteString(item.Topic);
                    buf.WriteByte((byte)item.Qos);
                }

                FixedHeader.RemaingLength = buf.WriterIndex;
                FixedHeader.WriteTo(buffer);
                buffer.WriteBytes(buf);
                buf = null;
            }
            finally
            {
                buf?.Release();
            }
        }

        public void Subscribe(string topic, MqttQos qos)
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
            public MqttQos Qos { get; set; }
        }
    }

    /// <summary>
    /// 订阅回执
    /// </summary>
    [PacketType(PacketType.SUBACK)]
    public class SubscribeAckPacket : Packet
    {
        public short MessageIdentifier { get; set; }

        public override void Decode(IByteBuffer buffer)
        {
            MessageIdentifier = buffer.ReadShort();
        }
    }
}