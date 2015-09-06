namespace nMqtt.Messages
{
    internal sealed class UnsubscribeMessage : MqttMessage
    {
        public UnsubscribeMessage()
            : base(MessageType.UNSUBSCRIBE)
        {
        }
    }

    internal sealed class UnsubscribeAckMessage : MqttMessage
    {
        public UnsubscribeAckMessage()
            : base(MessageType.UNSUBACK)
        {
        }
    }
}
