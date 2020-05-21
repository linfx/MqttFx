using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;

namespace MqttFx
{
    public class MqttConnectResult
    {
        public bool Succeeded { get; set; }

        public ConnectReturnCode ConnectReturn { get; set; }

        public IChannel Channel { get; set; }
    }
}
