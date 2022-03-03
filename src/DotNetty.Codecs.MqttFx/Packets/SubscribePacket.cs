using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅报文(SUBSCRIBE - Subscribe to topics)
    /// </summary>
    public sealed class SubscribePacket : PacketWithId
    {
        public SubscribePacket()
            : this(new PacketIdVariableHeader(), new SubscribePayload(new List<TopicSubscription>())) { }

        public SubscribePacket(ushort packetId, params TopicSubscription[] topics)
            : this(new PacketIdVariableHeader(packetId), new SubscribePayload(topics)) { }

        public SubscribePacket(PacketIdVariableHeader variableHeader, SubscribePayload payload)
            : base(variableHeader, payload)
        {
            VariableHeader = variableHeader;
            Payload = payload;
        }

        public IList<TopicSubscription> TopicSubscriptions
        {
            get { return ((SubscribePayload)Payload).TopicSubscriptions; }
            set { ((SubscribePayload)Payload).TopicSubscriptions = value; }
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public void AddSubscription(string topic, MqttQos qos)
        {
            TopicSubscription ts;
            ts.TopicName = topic;
            ts.Qos = qos;
            TopicSubscriptions.Add(ts);
        }
    }
}