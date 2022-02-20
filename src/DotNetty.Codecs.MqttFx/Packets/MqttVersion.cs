using System;

namespace DotNetty.Codecs.MqttFx.Packets
{
    [Flags]
    public enum MqttVersion : byte
    {
        MQTT_3_1,
        MQTT_3_1_1
    }
}
