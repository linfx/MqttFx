using DotNetty.Buffers;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 有效载荷(Payload)
/// </summary>
public sealed record class ConnectPayload : Payload
{
    /// <summary>
    /// 客户端标识符(Client Identifier)
    /// Each Client connecting to the Server has a unique ClientId. The ClientId MUST be used by Clients and by Servers to identify state that they hold relating to this MQTT Session between the Client and the Server [MQTT-3.1.3-2].
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// 遗嘱主题(Will Topic)
    /// The Will Topic MUST be a UTF-8 encoded string as defined in Section 1.5.3 [MQTT-3.1.3-10].
    /// </summary>
    public string WillTopic { get; set; }

    /// <summary>
    /// 遗嘱消息(Will Message)
    /// The Will Message defines the Application Message that is to be published to the Will Topic as described in Section 3.1.2.5. 
    /// </summary>
    public byte[] WillMessage { get; set; }

    /// <summary>
    /// 用户名(User Name)
    /// The User Name MUST be a UTF-8 encoded string as defined in Section 1.5.3 [MQTT-3.1.3-11].
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 密码(Password)
    /// The Password field contains 0 to 65535 bytes of binary data prefixed with a two byte length field which indicates the number of bytes used by the binary data (it does not include the two bytes taken up by the length field itself).
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="variableHeader"></param>
    public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
    {
        var connectVariableHeader = (ConnectVariableHeader)variableHeader;

        buffer.WriteString(ClientId);
        if (connectVariableHeader.ConnectFlags.WillFlag)
        {
            buffer.WriteString(WillTopic);
            buffer.WriteLengthBytes(WillMessage);
        }
        if (connectVariableHeader.ConnectFlags.UsernameFlag)
        {
            buffer.WriteString(UserName);
        }
        if (connectVariableHeader.ConnectFlags.PasswordFlag)
        {
            buffer.WriteString(Password);
        }
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="fixedHeader"></param>
    /// <param name="variableHeader"></param>
    public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
    {
        var connectVariableHeader = (ConnectVariableHeader)variableHeader;

        ClientId = buffer.ReadString(ref remainingLength);
        if (connectVariableHeader.ConnectFlags.WillFlag)
        {
            WillTopic = buffer.ReadString(ref remainingLength);
            WillMessage = buffer.ReadBytesArray(ref remainingLength);
        }
        if (connectVariableHeader.ConnectFlags.UsernameFlag)
        {
            UserName = buffer.ReadString(ref remainingLength);
        }
        if (connectVariableHeader.ConnectFlags.PasswordFlag)
        {
            Password = buffer.ReadString(ref remainingLength);
        }
    }
}
