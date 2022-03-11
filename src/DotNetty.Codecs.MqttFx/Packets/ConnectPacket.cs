namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 连接报文(CONNECT – Client requests a connection to a Server)
/// </summary>
public sealed record ConnectPacket : Packet
{
    /// <summary>
    /// 连接报文(CONNECT)
    /// </summary>
    public ConnectPacket()
        : this(new ConnectVariableHeader(), new ConnectPayload()) { }

    /// <summary>
    /// 连接报文(CONNECT)
    /// </summary>
    /// <param name="variableHeader"></param>
    /// <param name="payload"></param>
    public ConnectPacket(ConnectVariableHeader variableHeader, ConnectPayload payload)
        : base(variableHeader, payload) { }
}
