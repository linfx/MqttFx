using System;

namespace MqttFx
{
    public class MqttClientConnectedHandler : IMqttClientConnectedHandler
    {
        private readonly Action _handler;

        public MqttClientConnectedHandler(Action handler)
        {
            _handler = handler;
        }

        public void OnConnected()
        {
            _handler();
        }
    }
}
