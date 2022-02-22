using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 有效载荷(SUBSCRIBE Packet payload)
    /// </summary>
    public class SubscribePayload : Payload
    {
        /// <summary>
        /// 主题列表
        /// </summary>
        public List<TopicSubscription> TopicSubscriptions { get; set; } = new List<TopicSubscription>();

        public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
        {
            foreach (var item in TopicSubscriptions)
            {
                buffer.WriteString(item.TopicName);
                buffer.WriteByte((byte)item.Qos);
            }
        }

        public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
        {
        }
    }

    public struct TopicSubscription
    {
        /// <summary>
        /// Topic Filter
        /// </summary>
        public string TopicName;

        /// <summary>
        /// Requested QoS
        /// </summary>
        public MqttQos Qos;
    }
}
