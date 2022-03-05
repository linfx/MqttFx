namespace MqttFx.Client
{
    public interface IMessageReceivedHandler
    {
        void OnMesage(ApplicationMessage message);
    }
}