using System.IO;

namespace nMqtt.Messages
{
    internal sealed class DisconnectMessage : MqttMessage
    {
        public DisconnectMessage()
            : base(MessageType.DISCONNECT)
        {
        }
    }
}
