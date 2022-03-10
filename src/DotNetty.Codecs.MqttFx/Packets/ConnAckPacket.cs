namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 连接报文回执(CONNACK – Acknowledge connection request)
/// The CONNACK Packet is the packet sent by the Server in response to a CONNECT Packet received from a Client. 
/// The first packet sent from the Server to the Client MUST be a CONNACK Packet [MQTT-3.2.0-1].
/// </summary>
public sealed record class ConnAckPacket : Packet
{
    /// <summary>
    /// 连接报文回执(CONNACK)
    /// </summary>
    public ConnAckPacket()
        : this(new ConnAckVariableHeader()) { }

    /// <summary>
    /// 连接报文回执(CONNACK)
    /// </summary>
    /// <param name="variableHeader"></param>
    public ConnAckPacket(ConnAckVariableHeader variableHeader)
        : base(variableHeader) { }
}
