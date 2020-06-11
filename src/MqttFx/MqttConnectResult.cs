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
            get
            {
                if (ConnectReturn == ConnectReturnCode.ConnectionAccepted)
                    return true;

                return false;
            }
        }

        public ConnectReturnCode ConnectReturn { get; set; } = ConnectReturnCode.BrokerUnavailable;

        public IChannel Channel { get; set; }
    }
}
