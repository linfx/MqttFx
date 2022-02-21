namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// QoS2消息完成
    /// QoS 2 publish received, part 3
    /// </summary>
    public sealed class PubCompPacket : PacketWithId
    {
        public PubCompPacket(ushort packetId = default)
            : base(PacketType.PUBCOMP, packetId)
        {
        }
    }
}
