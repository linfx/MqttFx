namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 发布消息(PUBLISH – Publish message)
/// A PUBLISH Control Packet is sent from a Client to a Server or from Server to a Client to transport an Application Message.
/// </summary>
public sealed record PublishPacket : PacketWithId
{
    /// <summary>
    /// 发布消息(PUBLISH – Publish message)
    /// </summary>
    public PublishPacket()
        : this(new PublishVariableHeader(), new PublishPayload()) { }

    /// <summary>
    /// 发布消息(PUBLISH – Publish message)
    /// </summary>
    public PublishPacket(PublishPayload payload)
        : base(new PublishVariableHeader(), payload) { }

    /// <summary>
    /// 发布消息(PUBLISH – Publish message)
    /// </summary>
    public PublishPacket(PublishVariableHeader variableHeader, PublishPayload payload)
        : base(variableHeader, payload) { }

    /// <summary>
    /// 发布消息(PUBLISH – Publish message)
    /// </summary>
    public PublishPacket(FixedHeader fixedHeader, PublishVariableHeader variableHeader, PublishPayload payload)
        : base(fixedHeader, variableHeader, payload) { }

    /// <summary>
    /// 重发标志(DUP)
    /// 如果DUP标志被设置为0，表示这是客户端或服务端第一次请求发送这个PUBLISH报文。
    /// 如果DUP标志被设置为1，表示这可能是一个早前报文请求的重发。
    /// </summary>
    public bool Dup
    {
        get => this.GetDup();
        set => this.SetDup(value);
    }

    /// <summary>
    /// 服务质量等级(QoS)
    /// </summary>
    public MqttQos Qos
    {
        get => this.GetQos();
        set => this.SetQos(value);
    }

    /// <summary>
    /// 保留标志(RETAIN)
    /// </summary>
    public bool Retain
    {
        get => this.GetRetain();
        set => this.SetRetain(value);
    }

    /// <summary>
    /// 主题名称(UTF-8编码的字符串)(Topic Name)
    /// </summary>
    public string TopicName
    {
        get => ((PublishVariableHeader)VariableHeader).TopicName;
        set => ((PublishVariableHeader)VariableHeader).TopicName = value;
    }
}