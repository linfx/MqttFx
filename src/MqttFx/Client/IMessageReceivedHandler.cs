namespace MqttFx.Client
{
    public interface IMessageReceivedHandler
    {
        void OnMesage(Message message);
    }
}