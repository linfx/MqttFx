using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx
{
    public class Message
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }

        public MqttQos Qos { get; set; }

        public bool Retain { get; set; }
    }
}
