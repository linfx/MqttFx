using System.Collections.Generic;
using DotNetty.Buffers;
using nMqtt.Protocol;

namespace nMqtt.Packets
{
    /// <summary>
    /// 订阅主题
    /// </summary>
    [PacketType(PacketType.SUBSCRIBE)]
    public sealed class SubscribePacket : Packet, IMqttPacketIdentifier
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        List<TopicQos> Topics = new List<TopicQos>();

        /// <summary>
        /// 报文标识符
        /// </summary>
        public ushort PacketIdentifier { get; set; }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteUnsignedShort(PacketIdentifier);

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
    public class SubscribeAckPacket : Packet, IMqttPacketIdentifier
    {
        /// <summary>
        /// 报文标识符
        /// </summary>
        public ushort PacketIdentifier { get; set; }

        public override void Decode(IByteBuffer buffer)
        {
            PacketIdentifier = buffer.ReadUnsignedShort();
        }
    }
}