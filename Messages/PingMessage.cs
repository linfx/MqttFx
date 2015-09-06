using System.IO;

namespace nMqtt.Messages
{
    internal sealed class PingReqMessage : MqttMessage
    {
        public PingReqMessage()
            : base(MessageType.PINGREQ)
        {
        }

        public override void Encode(Stream stream)
        {
            FixedHeader.WriteTo(stream);
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
