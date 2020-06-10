namespace MqttFx
{
    public interface IMessageReceivedHandler
    {
        void OnMesage(Message message);
    }
}