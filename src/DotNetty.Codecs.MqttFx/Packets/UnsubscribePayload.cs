using DotNetty.Buffers;
using System.Collections.Generic;

namespace DotNetty.Codecs.MqttFx.Packets;

/// <summary>
/// 有效载荷(UNSUBSCRIBE Packet payload)
/// The payload for the UNSUBSCRIBE Packet contains the list of Topic Filters that the Client wishes to unsubscribe from. 
/// The Payload of an UNSUBSCRIBE packet MUST contain at least one Topic Filter. An UNSUBSCRIBE packet with no payload is a protocol violation [MQTT-3.10.3-2]. 
/// </summary>
public record UnsubscribePayload : Payload
{
    /// <summary>
    /// Topic Filters
    /// The Topic Filters in an UNSUBSCRIBE packet MUST be UTF-8 encoded strings as defined in Section 1.5.3, packed contiguously [MQTT-3.10.3-1].
    /// </summary>
    public IList<string> TopicFilters { get; set; }

    /// <summary>
    /// UNSUBSCRIBE Packet payload
    /// </summary>
    public UnsubscribePayload() { }

    /// <summary>
    /// UNSUBSCRIBE Packet payload
    /// </summary>
    /// <param name="topicFilters"></param>
    public UnsubscribePayload(params string[] topicFilters) => TopicFilters = topicFilters;

    public override void Encode(IByteBuffer buffer, VariableHeader variableHeader)
    {
        foreach (var item in TopicFilters)
        {
            buffer.WriteString(item);
        }
    }

    public override void Decode(IByteBuffer buffer, VariableHeader variableHeader, ref int remainingLength)
    {
        TopicFilters = new List<string>();

        while (remainingLength > 0)
        {
            string topicFilter = buffer.ReadString(ref remainingLength);
            MqttCodecUtil.ValidateTopicFilter(topicFilter);
            TopicFilters.Add(topicFilter);
        }

        if (TopicFilters.Count == 0)
            throw new DecoderException("The Payload of an UNSUBSCRIBE packet MUST contain at least one Topic Filter. An UNSUBSCRIBE packet with no payload is a protocol violation. [MQTT-3.10.3-2]");
    }
}
