namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// PING响应(PING response)
    /// </summary>
    public class PingRespPacket : Packet
    {
        public static readonly PingRespPacket Instance = new PingRespPacket();

        public PingRespPacket()
            : base(PacketType.PINGRESP) { }
    }
}