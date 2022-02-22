namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅报文(SUBSCRIBE - Subscribe to topics)
    /// </summary>
    public sealed class SubscribePacket : PacketWithId
    {
        public SubscribePacket()
            : this(new PacketIdVariableHeader(), new SubscribePayload())
        {
        }

        public SubscribePacket(PacketIdVariableHeader variableHeader, SubscribePayload payload)
            : base(variableHeader, payload)
        {
            FixedHeader.PacketType = PacketType.SUBSCRIBE;
            VariableHeader = variableHeader;
            Payload = payload;
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
            ((SubscribePayload)Payload).TopicSubscriptions.Add(ts);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="request"></param>
        public void AddSubscription(params TopicSubscription[] request) => ((SubscribePayload)Payload).TopicSubscriptions.AddRange(request);
    }
}