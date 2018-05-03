namespace nMqtt.Messages
{
    /// <summary>
    /// 断开连接
    /// </summary>
    [MessageType(MessageType.DISCONNECT)]
    internal sealed class DisconnectMessage : MqttMessage
    {
    }
}
