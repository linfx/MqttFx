namespace MqttFx
{
    public interface IMqttHandler
    {
        void OnMessage(Message message);
    }
}
