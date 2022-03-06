using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;

namespace MqttFx
{
    public class MqttConnectResult
    {
        public MqttConnectResult() { }

        public MqttConnectResult(ConnectReturnCode connectReturn)
        {
            ConnectReturn = connectReturn;
        }

        public bool Succeeded
        {
            get { return ConnectReturn == ConnectReturnCode.CONNECTION_ACCEPTED; }
        }

        public ConnectReturnCode ConnectReturn { get; set; } = ConnectReturnCode.CONNECTION_REFUSED_SERVER_UNAVAILABLE;

        public IChannel Channel { get; set; }
    }
}
