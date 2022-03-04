namespace DotNetty.Codecs.MqttFx.Packets
{
    /// <summary>
    /// 连接报文(CONNECT – Client requests a connection to a Server)
    /// </summary>
    public sealed class ConnectPacket : Packet
    {
        /// <summary>
        /// 连接报文
        /// </summary>
        public ConnectPacket()
            : this(new ConnectVariableHeader(), new ConnectPayload()) { }

        /// <summary>
        /// 连接报文
        /// </summary>
        /// <param name="variableHeader"></param>
        /// <param name="payload"></param>
        public ConnectPacket(ConnectVariableHeader variableHeader, ConnectPayload payload)
            : base(variableHeader, payload) { }
    }
}
