namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发布消息
    /// </summary>
    public sealed class PublishPacket : PacketWithId
    {
        /// <summary>
        /// 重发标志
        /// 如果DUP标志被设置为0，表示这是客户端或服务端第一次请求发送这个PUBLISH报文。
        /// 如果DUP标志被设置为1，表示这可能是一个早前报文请求的重发。
        /// </summary>
        public bool Dup
        {
            get { return FixedHeader.GetDup(); }
            private set { FixedHeader.SetDup(value); }
        }

        /// <summary>
        /// 服务质量等级
        /// </summary>
        public MqttQos Qos
        {
            get { return FixedHeader.GetQos(); }
            private set { FixedHeader.SetQos(value); }
        }

        /// <summary>
        /// 保留标志
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

        /// <summary>
        /// 发布消息
        /// </summary>
        public PublishPacket()
            : this(new PublishVariableHeader(), new PublishPayload())
        {
        }

        public PublishPacket(PublishVariableHeader variableHeader, PublishPayload payload)
            : base(variableHeader, payload)
        {
            FixedHeader.PacketType = PacketType.PUBLISH;
            VariableHeader = variableHeader;
            Payload = payload;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="qos">服务质量等级</param>
        /// <param name="dup">重发标志</param>
        /// <param name="retain">保留标志</param>
        public PublishPacket(MqttQos qos = MqttQos.AT_MOST_ONCE, bool dup = false, bool retain = false) : this()
        {
            // TODO: 不生效
            //Qos = qos;
            //Dup = dup;
            //Retain = retain;

            FixedHeader.Flags |= (byte)qos << 1;
            FixedHeader.Flags |= dup.ToByte() << 3;
            FixedHeader.Flags |= retain.ToByte();
        }

        ///// <summary>
        ///// 编码
        ///// </summary>
        ///// <param name="buffer"></param>
        //public override void Encode(IByteBuffer buffer)
        //{
        //    var buf = Unpooled.Buffer();
        //    try
        //    {
        //        buf.WriteString(TopicName);
        //        if (Qos > MqttQos.AT_MOST_ONCE)
        //            buf.WriteUnsignedShort(PacketIdentifier);
        //        buf.WriteBytes(Payload, 0, Payload.Length);

        //        FixedHeader.Encode(buffer, buf.ReadableBytes);
        //        buffer.WriteBytes(buf);
        //    }
        //    finally
        //    {
        //        buf?.Release();
        //    }
        //}

        ///// <summary>
        ///// 解码
        ///// </summary>
        ///// <param name="buffer"></param>
        //public override void Decode(IByteBuffer buffer)
        //{
        //    Dup = (FixedHeader.Flags & 0x08) == 0x08;
        //    Qos = (MqttQos)((FixedHeader.Flags & 0x06) >> 1);
        //    Retain = (FixedHeader.Flags & 0x01) > 0;

        //    //int remainingLength = fixedHeader.RemaingLength;
        //    //TopicName = buffer.ReadString(ref remainingLength);
        //    //if (fixedHeader.Qos > MqttQos.AtLeastOnce)
        //    //{
        //    //    PacketIdentifier = buffer.ReadUnsignedShort(ref remainingLength);
        //    //    if (PacketIdentifier == 0)
        //    //        throw new DecoderException("[MQTT-2.3.1-1]");
        //    //}
        //    //if (FixedHeader.RemaingLength > 0)
        //    //{
        //    //    Payload = new byte[FixedHeader.RemaingLength];
        //    //    buffer.ReadBytes(Payload, 0, FixedHeader.RemaingLength);
        //    //    FixedHeader.RemaingLength = 0;
        //    //}
        //}
    }
}
