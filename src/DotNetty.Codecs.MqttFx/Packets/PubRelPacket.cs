namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发布释放（QoS 2，第二步）
    /// </summary>
    public sealed class PubRelPacket : PacketWithIdentifier
    {
        public PubRelPacket(ushort packetId = default)
            : base(PacketType.PUBREL)
        {
            FixedHeader.Flags = 0x02;
            VariableHeader.PacketIdentifier = packetId;
        }
    }
}
