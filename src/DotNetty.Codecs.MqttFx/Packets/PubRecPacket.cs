namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 发布收到（QoS 2，第一步）
/// </summary>
public sealed record PubRecPacket : PacketWithId
{
    public PubRecPacket(ushort packetId = default)
        : base(packetId) { }
}
