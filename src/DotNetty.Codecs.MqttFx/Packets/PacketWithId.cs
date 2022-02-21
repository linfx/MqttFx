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
        /// <param name="packetType">报文类型</param>
        public PacketWithId(PacketType packetType)
            : this(packetType, new PacketIdVariableHeader())
        {
        }

        /// <summary>
        /// 报文抽象类(含报文标识符)(MQTT Control Packet)
        /// </summary>
        /// <param name="packetType">报文类型</param>
        public PacketWithId(PacketType packetType, ushort packetId)
            : this(packetType, new PacketIdVariableHeader(packetId))
        {
        }

        protected PacketWithId(PacketType packetType, PacketIdVariableHeader variableHeader)
            : base(variableHeader)
        {
            FixedHeader.PacketType = packetType;
            VariableHeader = variableHeader;
        }
    }
}
