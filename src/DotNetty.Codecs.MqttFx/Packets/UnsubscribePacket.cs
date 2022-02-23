namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 取消订阅(UNSUBSCRIBE – Unsubscribe from topics)
    /// </summary>
    public sealed class UnsubscribePacket : PacketWithId
    {
        public UnsubscribePacket()
            : this(new PacketIdVariableHeader(), new UnsubscribePayload()) { }

        public UnsubscribePacket(ushort packetId, params string[] topics)
            : this(new PacketIdVariableHeader(packetId), new UnsubscribePayload(topics)) { }

        public UnsubscribePacket(PacketIdVariableHeader variableHeader, UnsubscribePayload payload)
            : base(variableHeader, payload)
        {
            FixedHeader.PacketType = PacketType.UNSUBSCRIBE;
            VariableHeader = variableHeader;
            Payload = payload;
        }

        public string[] Topics
        {
            get { return ((UnsubscribePayload)Payload).Topics; }
            set { ((UnsubscribePayload)Payload).Topics = value; }
        }
    }
}
