namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 报文抽象类(含报文标识符)(MQTT Control Packet)
    /// </summary>
    public abstract class PacketWithId : Packet
    {
        /// <summary>
        /// 报文标识符(Packet Identifier)
        /// </summary>
        public ushort PacketId
        {
            get { return ((PacketIdVariableHeader)VariableHeader).PacketId; }
            set { ((PacketIdVariableHeader)VariableHeader).PacketId = value; }
        }

        /// <summary>
        /// 报文抽象类(含报文标识符)(MQTT Control Packet)
        /// </summary>
        /// <param name="packetType">报文类型</param>
        public PacketWithId(PacketType packetType)
            : this(packetType, new PacketIdVariableHeader()) { }

        /// <summary>
        /// 报文抽象类(含报文标识符)(MQTT Control Packet)
        /// </summary>
        /// <param name="packetType">报文类型</param>
        public PacketWithId(PacketType packetType, ushort packetId)
            : this(packetType, new PacketIdVariableHeader(packetId)) { }

        /// <summary>
        /// 报文抽象类(含报文标识符)(MQTT Control Packet)
        /// </summary>
        /// <param name="packetType">报文类型</param>
        protected PacketWithId(PacketType packetType, PacketIdVariableHeader variableHeader)
            : base(variableHeader)
        {
            FixedHeader.PacketType = packetType;
        }

        /// <summary>
        /// 报文抽象类(MQTT Control Packet)
        /// </summary>
        /// <param name="variableHeader">可变报头(Variable header)</param>
        /// <param name="payload">有效载荷(Payload)</param>
        protected PacketWithId(PacketIdVariableHeader variableHeader, Payload payload)
            : base(variableHeader, payload)
        {
        }
    }
}
