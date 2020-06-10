using System;

namespace MqttFx
{
    public static class MqttClientExtensions
    {
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