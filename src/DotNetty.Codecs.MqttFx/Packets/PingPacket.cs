namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// PING请求
    /// </summary>
    public sealed class PingReqPacket : Packet
    {
        public static readonly PingReqPacket Instance = new PingReqPacket();

        public PingReqPacket() 
            : base(PacketType.PINGREQ) { }
    }
}