namespace MqttFx.Client.Abstractions
{
    public interface IMessageReceivedHandler
    {
        void OnMesage(Message message);
    }
}