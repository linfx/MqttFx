namespace nMqtt.Packets
{
    /// <summary>
    /// PING请求
    /// </summary>
    [PacketType(PacketType.PINGREQ)]
    internal sealed class PingReqPacket : Packet
    {
    }

    /// <summary>
    /// PING响应
    /// </summary>
    [PacketType(PacketType.PINGRESP)]
    internal class PingRespPacket : Packet
    {
    }
}
