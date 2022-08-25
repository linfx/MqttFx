using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx;

public class TopicFilter
{
    /// <summary>
    ///     Gets or sets the MQTT topic.
    ///     In MQTT, the word topic refers to an UTF-8 string that the broker uses to filter messages for each connected
    ///     client.
    ///     The topic consists of one or more topic levels. Each topic level is separated by a forward slash (topic level
    ///     separator).
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    ///     Gets or sets the payload format indicator.
    ///     The payload format indicator is part of any MQTT packet that can contain a payload. The indicator is an optional
    ///     byte value.
    ///     A value of 0 indicates an “unspecified byte stream”.
    ///     A value of 1 indicates a "UTF-8 encoded payload".
    ///     If no payload format indicator is provided, the default value is 0.
    ///     Hint: MQTT 5 feature only.
    /// </summary>
    public MqttQos Qos { get; set; }
}
