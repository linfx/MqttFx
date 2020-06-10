using System;

namespace MqttFx
{
    public static class MqttClientExtensions
    {
        public static IMqttClient UseConnectedHandler(this IMqttClient client, Action handler)
        {
            return client.UseConnectedHandler(new MqttClientConnectedHandler(handler));
        }

        public static IMqttClient UseConnectedHandler(this IMqttClient client, IMqttClientConnectedHandler handler)
        {
            client.ConnectedHandler = handler;
            return client;
        }

        public static IMqttClient UseMessageReceivedHandler(this IMqttClient client, Action<Message> handler)
        {
            return client.UseApplicationMessageReceivedHandler(new MessageReceivedHandler(handler));
        }

        public static IMqttClient UseApplicationMessageReceivedHandler(this IMqttClient client, IMessageReceivedHandler handler)
        {
            client.MessageReceivedHandler = handler;
            return client;
        }
    }
}