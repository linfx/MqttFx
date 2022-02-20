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
            : base(PacketType.CONNACK) 
        {
            VariableHeader = new ConnAckVariableHeader();
        }

        public ConnAckPacket(FixedHeader fixedHeader, ConnAckVariableHeader variableHeader)
            : base(fixedHeader, variableHeader)
        {
        }
    }
}