namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// PING请求
    /// </summary>
    public sealed class PingReqPacket : Packet
    {
        public static readonly PingReqPacket Instance = new PingReqPacket();

        public PingReqPacket() 
            : base(PacketType.PINGREQ)
        {
        }
    }

    /// <summary>
    /// PING响应
    /// </summary>
    public class PingRespPacket : Packet
    {
        public static readonly PingRespPacket Instance = new PingRespPacket();

        public PingRespPacket() 
            : base(PacketType.PINGRESP)
        {
        }
    }
}