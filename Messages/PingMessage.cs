using System.IO;

namespace nMqtt.Messages
{
    internal sealed class PingReqMessage : MqttMessage
    {
        public PingReqMessage()
            : base(MessageType.PINGREQ)
        {
        }
    }

    internal class PingRespMessage : MqttMessage
    {
        public PingRespMessage()
            : base(MessageType.PINGRESP)
        {
        }
    }
}
