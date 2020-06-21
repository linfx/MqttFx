namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 发布收到（QoS 2，第一步）
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
