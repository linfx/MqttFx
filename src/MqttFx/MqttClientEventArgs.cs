using System;

namespace MqttFx
{
    public class MqttClientConnectedEventArgs : EventArgs
    {
        public MqttClientConnectedEventArgs(bool isSessionPresent)
        {
            IsSessionPresent = isSessionPresent;
        }

        public bool IsSessionPresent { get; }
    }

    public class MqttClientDisconnectedEventArgs : EventArgs
    {
        public MqttClientDisconnectedEventArgs(string clientId)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        }

        public string ClientId { get; }
    }

    public class MqttMessageReceivedEventArgs : EventArgs
    {
        public MqttMessageReceivedEventArgs(string clientId, Message message)
        {
            ClientId = clientId;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string ClientId { get; }

        public Message Message { get; }
    }
}
