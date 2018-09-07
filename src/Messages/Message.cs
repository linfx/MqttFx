using nMqtt.Protocol;

namespace nMqtt.Messages
{
    public class Message
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }

        public MqttQos Qos { get; set; }

        public bool Retain { get; set; }
    }
}
