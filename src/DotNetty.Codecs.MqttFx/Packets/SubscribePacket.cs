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
            FixedHeader.Qos = MqttQos.AtLeastOnce;
            FixedHeader.Retain = false;
            Payload.SubscribeTopics = new List<SubscribeRequest>();
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public void Add(string topic, MqttQos qos)
        {
            Payload.SubscribeTopics.Add(new SubscribeRequest(topic, qos));
        }

        public void AddRange(params SubscribeRequest[] request)
        {
            Payload.SubscribeTopics.AddRange(request);
        }

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                VariableHeader.Encode(buf, FixedHeader);
                Payload.Encode(buf);
                FixedHeader.Encode(buffer, buf.ReadableBytes);
                buffer.WriteBytes(buf);
            }
            finally
            {
                buf?.Release();
            }
        }
    }
}