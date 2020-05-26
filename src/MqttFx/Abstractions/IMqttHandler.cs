namespace MqttFx
{
    public interface IMqttHandler
    {
        void OnMesage(string topic, byte[] payload);
    }
}
