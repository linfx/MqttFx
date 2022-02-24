using System;

namespace DotNetty.Codecs.MqttFx.Packets
{
    [Flags]
    public enum MqttVersion : byte
    {
        MQTT_3_1   = 0x03,
        MQTT_3_1_1 = 0x04,
        MQTT_5     = 0x05,
    }
}
