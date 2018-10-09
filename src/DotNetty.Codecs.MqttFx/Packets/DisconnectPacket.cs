namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 断开连接
    /// </summary>
    public sealed class DisconnectPacket : Packet
    {
        public DisconnectPacket() 
            : base(PacketType.DISCONNECT)
        {
        }
    }
}
