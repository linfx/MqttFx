using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 可变报头(Variable header)
/// The variable header for the CONNECT Packet consists of four fields in the following order:  Protocol Name, Protocol Level, Connect Flags, and Keep Alive.
/// </summary>
public sealed class ConnectVariableHeader : VariableHeader
{
    /// <summary>
    /// 协议名(UTF-8编码的字符串)(Protocol Name)
    /// </summary>
    public string ProtocolName { get; set; } = "MQTT";

    /// <summary>
    /// 协议级别(Protocol Level)
    /// The 8 bit unsigned value that represents the revision level of the protocol used by the Client. 
    /// The value of the Protocol Level field for the version 3.1.1 of the protocol is 4 (0x04). 
    /// The Server MUST respond to the CONNECT Packet with a CONNACK return code 0x01 (unacceptable protocol level) and then disconnect the Client if the Protocol Level is not supported by the Server [MQTT-3.1.2-2].
    /// </summary>
    public byte ProtocolLevel { get; set; } = (byte)MqttVersion.MQTT_3_1_1;

    /// <summary>
    /// The Connect Flags byte contains a number of parameters specifying the behavior of the MQTT connection. It also indicates the presence or absence of fields in the payload.
    /// </summary>
    public ConnectFlags ConnectFlags;

    /// <summary>
    /// 保持连接(Keep Alive)
    /// 以秒为单位的时间间隔，表示为一个16位的字，它是指在客户端传输完成一个控制报文的时刻到发送下一个报文的时刻，两者之间允许空闲的最大时间间隔。
    /// The Keep Alive is a time interval measured in seconds. 
    /// Expressed as a 16-bit word, it is the maximum time interval that is permitted to elapse between the point at which the Client finishes transmitting one Control Packet and the point it starts sending the next. 
    /// It is the responsibility of the Client to ensure that the interval between Control Packets being sent does not exceed the Keep Alive value. 
    /// In the absence of sending any other Control Packets, the Client MUST send a PINGREQ Packet [MQTT-3.1.2-23].
    /// </summary>
    public ushort KeepAlive { get; set; }

    /// <summary>
    /// CONNECT报文的可变报头按下列次序包含四个字段：
    /// 协议名（Protocol Name）
    /// 协议级别（Protocol Level）
    /// 连接标志（Connect Flags）
    /// 保持连接（Keep Alive）
    /// </summary>
    /// <param name="buffer"></param>
    public override void Encode(IByteBuffer buffer)
    {
        // 协议名 Protocol Name
        buffer.WriteString(ProtocolName);

        // 协议级别 Protocol Level
        buffer.WriteByte(ProtocolLevel);

        // 连接标志 Connect Flags                          
        var connectFlags = ConnectFlags.UsernameFlag.ToByte() << 7;
        connectFlags |= ConnectFlags.PasswordFlag.ToByte() << 6;
        connectFlags |= ConnectFlags.WillRetain.ToByte() << 5;
        connectFlags |= ((byte)ConnectFlags.WillQos) << 3;
        connectFlags |= ConnectFlags.WillFlag.ToByte() << 2;
        connectFlags |= ConnectFlags.CleanSession.ToByte() << 1;
        buffer.WriteByte(connectFlags);

        // 保持连接 Keep Alive
        buffer.WriteShort(KeepAlive);
    }

    public override void Decode(IByteBuffer buffer, ref FixedHeader fixedHeader)
    {
        // 协议名 Protocol Name
        ProtocolName = buffer.ReadString(ref fixedHeader.RemainingLength);

        // 协议级别 Protocol Level
        ProtocolLevel = buffer.ReadByte(ref fixedHeader.RemainingLength);

        // 连接标志 Connect Flags
        int connectFlags = buffer.ReadByte(ref fixedHeader.RemainingLength);
        ConnectFlags.CleanSession = (connectFlags & 0x02) == 0x02;
        ConnectFlags.WillFlag = (connectFlags & 0x04) == 0x04;
        if (ConnectFlags.WillFlag)
        {
            ConnectFlags.WillQos = (MqttQos)((connectFlags & 0x18) >> 3);
            ConnectFlags.WillRetain = (connectFlags & 0x20) == 0x20;
        }
        ConnectFlags.UsernameFlag = (connectFlags & 0x80) == 0x80;
        ConnectFlags.PasswordFlag = (connectFlags & 0x40) == 0x40;

        // 保持连接 Keep Alive
        KeepAlive = (ushort)buffer.ReadShort(ref fixedHeader.RemainingLength);
    }
}
