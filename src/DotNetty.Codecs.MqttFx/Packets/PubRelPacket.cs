namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 发布释放（QoS 2，第二步）
/// </summary>
public sealed record PubRelPacket : PacketWithId
{
    public PubRelPacket(ushort packetId = default)
        : base(packetId) { }
}
