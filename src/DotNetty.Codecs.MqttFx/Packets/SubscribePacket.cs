namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 订阅报文
    /// </summary>
    public sealed class SubscribePacket : Packet
    {
        public SubscribePacket()
            : this(new PacketIdVariableHeader(), new SubscribePayload())
        {
        }

        public SubscribePacket(PacketIdVariableHeader variableHeader, SubscribePayload payload)
            : base(variableHeader, payload)
        {
            FixedHeader.PacketType = PacketType.CONNECT;
            VariableHeader = variableHeader;
            Payload = payload;
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qos">服务质量等级</param>
        public void Add(string topic, MqttQos qos) => ((SubscribePayload)Payload).SubscribeTopics.Add(new SubscribeRequest(topic, qos));

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="request"></param>
        public void AddRange(params SubscribeRequest[] request) => ((SubscribePayload)Payload).SubscribeTopics.AddRange(request);
    }
}