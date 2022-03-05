namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 报文抽象类(含报文标识符)(MQTT Control Packet)
    /// </summary>
    public abstract class PacketWithId : Packet
    {
        /// <summary>
        /// 报文抽象类(含报文标识符)(MQTT Control Packet)
        /// </summary>
        public PacketWithId()
            : this(new PacketIdVariableHeader()) { }

        /// <summary>
        /// 报文抽象类(含报文标识符)(MQTT Control Packet)
        /// </summary>
        public PacketWithId(ushort packetId)
            : this(new PacketIdVariableHeader(packetId)) { }

        /// <summary>
        /// 报文抽象类(含报文标识符)(MQTT Control Packet)
        /// </summary>
        protected PacketWithId(PacketIdVariableHeader variableHeader)
            : base(variableHeader) { }

        /// <summary>
        /// 报文抽象类(MQTT Control Packet)
        /// </summary>
        /// <param name="variableHeader">可变报头(Variable header)</param>
        /// <param name="payload">有效载荷(Payload)</param>
        protected PacketWithId(PacketIdVariableHeader variableHeader, Payload payload)
            : base(variableHeader, payload) { }

        /// <summary>
        /// 报文标识符(Packet Identifier)
        /// </summary>
        public ushort PacketId
        {
            get => ((PacketIdVariableHeader)VariableHeader).PacketId;
            set => ((PacketIdVariableHeader)VariableHeader).PacketId = value;
        }
    }
}
