namespace nMqtt.Packets
{
    /// <summary>
    /// 断开连接
    /// </summary>
    [PacketType(PacketType.DISCONNECT)]
    public sealed class DisconnectPacket : Packet
    {
    }
}
