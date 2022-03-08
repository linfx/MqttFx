namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发布消息(PUBLISH – Publish message)
    /// A PUBLISH Control Packet is sent from a Client to a Server or from Server to a Client to transport an Application Message.
    /// </summary>
    public sealed class PublishPacket : PacketWithId
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        public PublishPacket()
            : this(new PublishVariableHeader(), new PublishPayload()) { }

        public PublishPacket(PublishVariableHeader variableHeader, PublishPayload payload)
            : base(variableHeader, payload) { }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="qos">服务质量等级</param>
        /// <param name="dup">重发标志</param>
        /// <param name="retain">保留标志</param>
        public PublishPacket(MqttQos qos = MqttQos.AtMostOnce, bool dup = false, bool retain = false) 
            : this()
        {
            FixedHeader.Flags |= (byte)qos << 1;
            FixedHeader.Flags |= dup.ToByte() << 3;
            FixedHeader.Flags |= retain.ToByte();
        }

        /// <summary>
        /// 重发标志(DUP)
        /// 如果DUP标志被设置为0，表示这是客户端或服务端第一次请求发送这个PUBLISH报文。
        /// 如果DUP标志被设置为1，表示这可能是一个早前报文请求的重发。
        /// </summary>
        public bool Dup => FixedHeader.GetDup();

        /// <summary>
        /// 服务质量等级(QoS)
        /// </summary>
        public MqttQos Qos => FixedHeader.GetQos();

        /// <summary>
        /// 保留标志(RETAIN)
        /// </summary>
        public bool Retain => FixedHeader.GetRetain();

        /// <summary>
        /// 主题名称(UTF-8编码的字符串)(Topic Name)
        /// </summary>
        public string TopicName
        {
            get => ((PublishVariableHeader)VariableHeader).TopicName;
            set => ((PublishVariableHeader)VariableHeader).TopicName = value;
        }

        public void SetPayload(byte[] payload)
        {
            ((PublishPayload)Payload).Body = payload;
        }
    }
}
