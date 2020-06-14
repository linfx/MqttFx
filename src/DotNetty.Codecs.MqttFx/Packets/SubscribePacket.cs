using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅报文
    /// </summary>
    public sealed class SubscribePacket : PacketWithIdentifier
    {
        /// <summary>
        /// 有效载荷
        /// </summary>
        public SubscribePayload Payload;

        public SubscribePacket()
            : base(PacketType.SUBSCRIBE) 
        {
            Payload.SubscribeTopics = new List<SubscriptionRequest>();
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public void Add(string topic, MqttQos qos)
        {
            Payload.SubscribeTopics.Add(new SubscriptionRequest(topic, qos));
        }

        public void AddRange(params SubscriptionRequest[] request)
        {
            Payload.SubscribeTopics.AddRange(request);
        }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                buf.WriteUnsignedShort(VariableHeader.PacketIdentifier);

                foreach (var item in Payload.SubscribeTopics)
                {
                    buf.WriteString(item.Topic);
                    buf.WriteByte((byte)item.Qos);
                }

                VariableHeader.Encode(buf, FixedHeader);

                FixedHeader.RemaingLength = buf.ReadableBytes;
                FixedHeader.Encode(buffer);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.Release();
            }
        }
    }
}