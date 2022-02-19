namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// PING响应
    /// </summary>
    public class PingRespPacket : Packet
    {
        public static readonly PingRespPacket Instance = new PingRespPacket();

        public PingRespPacket()
            : base(PacketType.PINGRESP) { }
    }
}