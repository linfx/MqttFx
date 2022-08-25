namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 订阅回执(SUBACK – Subscribe acknowledgement)
/// </summary>
public sealed record SubAckPacket : PacketWithId
{
    public SubAckPacket()
        : this(new PacketIdVariableHeader(), new SubAckPayload()) { }

    public SubAckPacket(ushort packetId, params MqttQos[] returnCodes)
        : this(new PacketIdVariableHeader(packetId), new SubAckPayload(returnCodes)) { }

    public SubAckPacket(PacketIdVariableHeader variableHeader, SubAckPayload payload)
        : base(variableHeader, payload) { }

    /// <summary>
    /// 返回代码
    /// </summary>
    public MqttQos[] ReturnCodes => ((SubAckPayload)Payload).ReturnCodes;
}
