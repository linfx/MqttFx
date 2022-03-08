using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;

namespace MqttFx
{
    public class ConnectResult
    {
        public ConnectResult() { }

        public ConnectResult(ConnectReturnCode connectReturn)
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
