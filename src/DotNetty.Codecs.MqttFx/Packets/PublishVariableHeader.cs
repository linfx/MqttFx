using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 可变报头
/// </summary>
public record PublishVariableHeader : PacketIdVariableHeader
{
    /// <summary>
    /// 主题名称(UTF-8编码的字符串)(Topic Name)
    /// 附加到应用程序消息的标签，该标签与服务器已知的订阅相匹配。服务器将应用程序消息的副本发送到具有匹配订阅的每个客户端。
    /// The label attached to an Application Message which is matched against the Subscriptions known to the Server. The Server sends a copy of the Application Message to each Client that has a matching Subscription.
    /// </summary>
    public string TopicName { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="fixedHeader"></param>
    public override void Encode(IByteBuffer buffer, FixedHeader fixedHeader)
    {
        buffer.WriteString(TopicName);
        if (fixedHeader.GetQos() > MqttQos.AtMostOnce)
            base.Encode(buffer, fixedHeader);
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="fixedHeader"></param>
    public override void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader)
    {
        TopicName = buffer.ReadString(ref fixedHeader.RemainingLength);
        if (fixedHeader.GetQos() > MqttQos.AtMostOnce)
            base.Decode(buffer, ref fixedHeader);
    }
}
