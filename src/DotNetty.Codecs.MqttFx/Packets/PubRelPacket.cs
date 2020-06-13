namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// QoS2消息释放
    /// QoS 2 publish received, part 2
    /// </summary>
    public sealed class PubRelPacket : PacketWithId
    {
        public PubRelPacket(ushort packetId = default)
            : base(PacketType.PUBREL)
        {
            PacketId = packetId;
        }
    }
}
