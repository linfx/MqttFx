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
            FixedHeader.Flags = 0x02;
            Payload.SubscribeTopics = new List<SubscribeRequest>();
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public void Add(string topic, MqttQos qos) => Payload.SubscribeTopics.Add(new SubscribeRequest(topic, qos));

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="request"></param>
        public void AddRange(params SubscribeRequest[] request) => Payload.SubscribeTopics.AddRange(request);

        public override void Encode(IByteBuffer buffer)
        {
            var buf = Unpooled.Buffer();
            try
            {
                // Variable header
                VariableHeader.Encode(buf, FixedHeader);

                // Payload
                Payload.Encode(buf);

                // Fixed header
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