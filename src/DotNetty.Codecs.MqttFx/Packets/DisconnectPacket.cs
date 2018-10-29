namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 断开连接
    /// </summary>
    public sealed class DisconnectPacket : Packet
    {
        public static readonly DisconnectPacket Instance = new DisconnectPacket();

        public DisconnectPacket() 
            : base(PacketType.DISCONNECT)
        {
        }
    }
}
