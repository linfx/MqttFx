namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接报文回执
    /// </summary>
    public sealed class ConnAckPacket : Packet
    {
        /// <summary>
        /// 连接报文回执
        /// </summary>
        public ConnAckPacket()
            : this(new ConnAckVariableHeader()) 
        {
        }

        public ConnAckPacket(ConnAckVariableHeader variableHeader)
            : base(variableHeader)
        {
            FixedHeader.PacketType = PacketType.CONNACK;
            VariableHeader = variableHeader;
        }
    }
}