using nMqtt.Protocol;

namespace nMqtt.Packets
{
    /// <summary>
    /// 断开连接
    /// </summary>
    [PacketType(PacketType.DISCONNECT)]
    internal sealed class DisconnectPacket : Packet
    {
    }
}
