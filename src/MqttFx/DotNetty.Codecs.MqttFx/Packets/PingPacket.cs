namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// PING请求
    /// </summary>
    internal sealed class PingReqPacket : Packet
    {
        public PingReqPacket() 
            : base(PacketType.PINGREQ)
        {
        }
    }

    /// <summary>
    /// PING响应
    /// </summary>
    internal class PingRespPacket : Packet
    {
        public PingRespPacket() 
            : base(PacketType.PINGRESP)
        {
        }
    }
}