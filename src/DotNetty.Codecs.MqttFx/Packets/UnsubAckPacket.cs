namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 取消订阅回执(UNSUBACK – Unsubscribe acknowledgement)
/// The UNSUBACK Packet is sent by the Server to the Client to confirm receipt of an UNSUBSCRIBE Packet.
/// </summary>
public sealed record class UnsubAckPacket : PacketWithId
{
}
