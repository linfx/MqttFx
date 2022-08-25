namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 断开连接(DISCONNECT – Disconnect notification)
/// The DISCONNECT Packet is the final Control Packet sent from the Client to the Server. It indicates that the Client is disconnecting cleanly.
/// </summary>
public sealed record DisconnectPacket : Packet
{
    public static readonly DisconnectPacket Instance = new DisconnectPacket();
}
