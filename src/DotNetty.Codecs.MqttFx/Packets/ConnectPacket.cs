namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接报文
    /// </summary>
    public sealed class ConnectPacket : Packet
    {
        /// <summary>
        /// 连接报文
        /// </summary>
        public ConnectPacket()
            : this(new ConnectVariableHeader(), new ConnectPlayload())
        {
        }

        public ConnectPacket(ConnectVariableHeader variableHeader, ConnectPlayload payload)
            : base(variableHeader, payload)
        {
            FixedHeader.PacketType = PacketType.CONNECT;
            VariableHeader = variableHeader;
            Payload = payload;
        }
    }
}
