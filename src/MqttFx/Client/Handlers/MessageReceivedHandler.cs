using System;

namespace MqttFx.Client.Handlers
{
    public class MessageReceivedHandler : IMessageReceivedHandler
    {
        private readonly Action<ApplicationMessage> _handler;

        public MessageReceivedHandler(Action<ApplicationMessage> handler)
        {
            _handler = handler;
        }

        public void OnMesage(ApplicationMessage message)
        {
            _handler(message);
        }
    }
}
