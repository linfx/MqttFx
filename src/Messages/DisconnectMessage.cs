namespace nMqtt.Messages
{
    /// <summary>
    /// 断开连接
    /// </summary>
    [MessageType(MessageType.DISCONNECT)]
    public sealed class DisconnectMessage : MqttMessage
    {
    }
}
