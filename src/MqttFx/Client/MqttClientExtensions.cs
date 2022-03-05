using MqttFx.Client.Handlers;
using System;

namespace MqttFx.Client
{
    public static class MqttClientExtensions
    {
        public static MqttClient UseConnectedHandler(this MqttClient client, Action handler)
        {
            return client.UseConnectedHandler(new MqttClientConnectedHandler(handler));
        }

        public static MqttClient UseConnectedHandler(this MqttClient client, IMqttClientConnectedHandler handler)
        {
            client.ConnectedHandler = handler;
            return client;
        }

        public static MqttClient UseDisconnectedHandler(this MqttClient client, Action handler)
        {
            return client.UseDisconnectedHandler(new MqttClientDisconnectedHandler(handler));
        }

        public static MqttClient UseDisconnectedHandler(this MqttClient client, IMqttClientDisconnectedHandler handler)
        {
            client.DisconnectedHandler = handler;
            return client;
        }

        public static MqttClient UseMessageReceivedHandler(this MqttClient client, Action<ApplicationMessage> handler)
        {
            return client.UseApplicationMessageReceivedHandler(new MessageReceivedHandler(handler));
        }

        public static MqttClient UseApplicationMessageReceivedHandler(this MqttClient client, IMessageReceivedHandler handler)
        {
            client.MessageReceivedHandler = handler;
            return client;
        }
    }
}