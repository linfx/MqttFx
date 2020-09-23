using System;

namespace MqttFx.Client.Handlers
{
    public class MqttClientDisconnectedHandler : IMqttClientDisconnectedHandler
    {
        private readonly Action _handler;

        public MqttClientDisconnectedHandler(Action handler)
        {
            _handler = handler;
        }

        public void OnConnected()
        {
            _handler();
        }
    }
}
