namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// PING响应(PING response)
    /// A PINGRESP Packet is sent by the Server to the Client in response to a PINGREQ Packet. It indicates that the Server is alive.
    /// </summary>
    public class PingRespPacket : Packet
    {
        public static readonly PingRespPacket Instance = new PingRespPacket();
    }
}