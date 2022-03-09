using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx
{
    public class ConnectResult
    {
        public ConnectResult(ConnectReturnCode connectReturn)
        {
            ConnectReturn = connectReturn;
        }

        public bool Succeeded => ConnectReturn == ConnectReturnCode.CONNECTION_ACCEPTED;

        public ConnectReturnCode ConnectReturn { get; set; } = ConnectReturnCode.CONNECTION_REFUSED_SERVER_UNAVAILABLE;
    }
}
