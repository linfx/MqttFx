namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 发布回执
/// QoS level = 1
/// </summary>
public sealed record class PubAckPacket : PacketWithId
{
    public PubAckPacket(ushort packetId = default)
        : base(packetId) { }
}
