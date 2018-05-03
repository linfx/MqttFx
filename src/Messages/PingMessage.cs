namespace nMqtt.Messages
{
    /// <summary>
    /// PING请求
    /// </summary>
    [MessageType(MessageType.PINGREQ)]
    internal sealed class PingReqMessage : MqttMessage
    {
    }

    /// <summary>
    /// PING响应
    /// </summary>
    [MessageType(MessageType.PINGRESP)]
    internal class PingRespMessage : MqttMessage
    {
    }
}
