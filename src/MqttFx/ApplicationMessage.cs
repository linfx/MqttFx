using DotNetty.Codecs.MqttFx.Packets;

namespace MqttFx;

/// <summary>
/// 应用程序消息(Application Message)
/// MQTT 协议通过网络为应用程序携带的数据。当应用程序消息由 MQTT 传输时，它们具有关联的服务质量和主题名称。
/// The data carried by the MQTT protocol across the network for the application. When Application Messages are transported by MQTT they have an associated Quality of Service and a Topic Name.
/// </summary>
public class ApplicationMessage
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
    ///     Gets or sets the payload.
    ///     The payload is the data bytes sent via the MQTT protocol.
    /// </summary>
    public byte[] Payload { get; set; }

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

    /// <summary>
    ///     If the DUP flag is set to 0, it indicates that this is the first occasion that the Client or Server has attempted
    ///     to send this MQTT PUBLISH Packet.
    ///     If the DUP flag is set to 1, it indicates that this might be re-delivery of an earlier attempt to send the Packet.
    ///     The DUP flag MUST be set to 1 by the Client or Server when it attempts to re-deliver a PUBLISH Packet
    ///     [MQTT-3.3.1.-1].
    ///     The DUP flag MUST be set to 0 for all QoS 0 messages [MQTT-3.3.1-2].
    /// </summary>
    public bool Dup { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the message should be retained or not.
    ///     A retained message is a normal MQTT message with the retained flag set to true.
    ///     The broker stores the last retained message and the corresponding QoS for that topic.
    /// </summary>
    public bool Retain { get; set; }
}
