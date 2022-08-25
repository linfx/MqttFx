using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
/// An UNSUBSCRIBE Packet is sent by the Client to the Server, to unsubscribe from topics.
/// </summary>
public sealed record UnsubscribePacket : PacketWithId
{
    /// <summary>
    /// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
    /// </summary>
    public UnsubscribePacket()
        : this(new PacketIdVariableHeader(), new UnsubscribePayload()) { }

    /// <summary>
    /// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
    /// </summary>
    /// <param name="packetId"></param>
    /// <param name="topicFilters"></param>
    public UnsubscribePacket(ushort packetId, params string[] topicFilters)
        : this(new PacketIdVariableHeader(packetId), new UnsubscribePayload(topicFilters)) { }

    /// <summary>
    /// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
    /// </summary>
    /// <param name="variableHeader"></param>
    /// <param name="payload"></param>
    public UnsubscribePacket(PacketIdVariableHeader variableHeader, UnsubscribePayload payload)
        : base(variableHeader, payload) { }

    /// <summary>
    /// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
    /// </summary>
    /// <param name="fixedHeader"></param>
    /// <param name="variableHeader"></param>
    /// <param name="payload"></param>
    public UnsubscribePacket(FixedHeader fixedHeader, PacketIdVariableHeader variableHeader, UnsubscribePayload payload)
        : base(fixedHeader, variableHeader, payload) { }

    public IList<string> TopicFilters
    {
        get => ((UnsubscribePayload)Payload).TopicFilters;
        set => ((UnsubscribePayload)Payload).TopicFilters = value;
    }
}
