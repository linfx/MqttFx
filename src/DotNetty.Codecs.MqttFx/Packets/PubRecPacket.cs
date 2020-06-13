namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// QoS2消息回执
    /// QoS 2 publish received, part 1
    /// </summary>
    public sealed class PubRecPacket : PacketWithIdentifier
    {
        public PubRecPacket(ushort packetId = default)
            : base(PacketType.PUBREC)
        {
            VariableHeader.PacketIdentifier = packetId;
        }
    }
}
