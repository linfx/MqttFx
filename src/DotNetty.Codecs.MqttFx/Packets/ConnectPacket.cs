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
            : base(PacketType.CONNECT)
        {
            VariableHeader = new ConnectVariableHeader();
            Payload = new ConnectPlayload();
        }

        public ConnectPacket(FixedHeader fixedHeader, ConnectVariableHeader variableHeader, ConnectPlayload payload)
            : base(fixedHeader, variableHeader)
        {
            VariableHeader = variableHeader;
            Payload = payload;
        }
    }
}
