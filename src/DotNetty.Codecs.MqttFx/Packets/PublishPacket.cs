namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发布消息(PUBLISH – Publish message)
    /// </summary>
    public sealed class PublishPacket : PacketWithId
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        public PublishPacket()
            : this(new PublishVariableHeader(), new PublishPayload()) { }

        public PublishPacket(PublishVariableHeader variableHeader, PublishPayload payload)
            : base(variableHeader, payload)
        {
            VariableHeader = variableHeader;
            Payload = payload;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="qos">服务质量等级</param>
        /// <param name="dup">重发标志</param>
        /// <param name="retain">保留标志</param>
        public PublishPacket(MqttQos qos = MqttQos.AT_MOST_ONCE, bool dup = false, bool retain = false) 
            : this()
        {
            // TODO: 不生效
            //Qos = qos;
            //Dup = dup;
            //Retain = retain;

            FixedHeader.Flags |= (byte)qos << 1;
            FixedHeader.Flags |= dup.ToByte() << 3;
            FixedHeader.Flags |= retain.ToByte();
        }

        /// <summary>
        /// 重发标志(DUP)
        /// 如果DUP标志被设置为0，表示这是客户端或服务端第一次请求发送这个PUBLISH报文。
        /// 如果DUP标志被设置为1，表示这可能是一个早前报文请求的重发。
        /// </summary>
        public bool Dup
        {
            get { return FixedHeader.GetDup(); }
            private set { FixedHeader.SetDup(value); }
        }

        /// <summary>
        /// 服务质量等级(QoS)
        /// </summary>
        public MqttQos Qos
        {
            get { return FixedHeader.GetQos(); }
            private set { FixedHeader.SetQos(value); }
        }

        /// <summary>
        /// 保留标志(RETAIN)
        /// </summary>
        public bool Retain
        {
            get { return FixedHeader.GetRetain(); }
            private set { FixedHeader.SetRetain(value); }
        }

        /// <summary>
        /// 主题名(UTF-8编码的字符串)(Topic Name)
        /// </summary>
        public string TopicName
        {
            get { return ((PublishVariableHeader)VariableHeader).TopicName; }
            set { ((PublishVariableHeader)VariableHeader).TopicName = value; }
        }
    }
}
