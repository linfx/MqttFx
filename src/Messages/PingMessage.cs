namespace nMqtt.Messages
{
    /// <summary>
    /// PING请求
    /// </summary>
    [MessageType(MqttMessageType.PINGREQ)]
    internal sealed class PingReqMessage : MqttMessage
    {
    }

    /// <summary>
    /// PING响应
    /// </summary>
    [MessageType(MqttMessageType.PINGRESP)]
    internal class PingRespMessage : MqttMessage
    {
    }
}
