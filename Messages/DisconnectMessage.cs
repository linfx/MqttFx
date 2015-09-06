using System.IO;

namespace nMqtt.Messages
{
    internal sealed class DisconnectMessage : MqttMessage
    {
        public DisconnectMessage()
            : base(MessageType.DISCONNECT)
        {
        }

        public override void Encode(Stream stream)
        {
            FixedHeader.WriteTo(stream);
        }
    }
}
