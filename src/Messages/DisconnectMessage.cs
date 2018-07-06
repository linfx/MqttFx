namespace nMqtt.Messages
{
    /// <summary>
    /// 断开连接
    /// </summary>
    [MessageType(MqttMessageType.DISCONNECT)]
    public sealed class DisconnectMessage : MqttMessage
    {
    }
}
