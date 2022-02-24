namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// PING请求(PING request)
    /// The PINGREQ Packet is sent from a Client to the Server. It can be used to:
    /// 1. Indicate to the Server that the Client is alive in the absence of any other Control Packets being sent from the Client to the Server.
    /// 2. Request that the Server responds to confirm that it is alive.
    /// 3. Exercise the network to indicate that the Network Connection is active.
    /// </summary>
    public sealed class PingReqPacket : Packet
    {
        public static readonly PingReqPacket Instance = new PingReqPacket();

        public PingReqPacket() 
            : base(PacketType.PINGREQ) { }
    }
}