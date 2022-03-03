namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接报文回执(CONNACK – Acknowledge connection request)
    /// </summary>
    public sealed class ConnAckPacket : Packet
    {
        /// <summary>
        /// 连接报文回执
        /// </summary>
        public ConnAckPacket()
            : this(new ConnAckVariableHeader()) { }

        public ConnAckPacket(ConnAckVariableHeader variableHeader)
            : base(variableHeader)
        {
            VariableHeader = variableHeader;
        }
    }
}